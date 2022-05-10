/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CNS.Auth.Web.Services
{
	public class SAMLService : ISAMLService
	{
		private readonly ILogger<SAMLService> log;
		private readonly Saml2SecurityTokenHandler samlTokenHandler;
		private readonly SAMLServiceOptions options;

		public SAMLService(ILogger<SAMLService> log, IOptions<SAMLServiceOptions> options, Saml2SecurityTokenHandler samlTokenHandler)
		{
			this.log = log ?? throw new ArgumentNullException(nameof(log));
			this.samlTokenHandler = samlTokenHandler ?? throw new ArgumentNullException(nameof(samlTokenHandler));
			this.options = options.Value;
		}

		/// <summary>
		/// Create a SAML response by XML document and principal claims
		/// </summary>
		/// <param name="SAMLRequest">Xml document object</param>
		/// <param name="principal">Principal Claims</param>
		/// <param name="sign">bool value for sign status</param>
		/// <param name="cancellationToken">cancellation token</param>
		/// <returns></returns>
		public async Task<XDocument> CreateSAMLResponse(XDocument SAMLRequest, ClaimsPrincipal principal, bool sign = false, CancellationToken cancellationToken = default)
		{
			string inResponseTo = SAMLRequest.Root.Attribute("ID")?.Value;
			string audience = SAMLRequest.Descendants(XName.Get(Saml2Constants.Elements.Issuer, Saml2Constants.Namespace)).First().Value;
			string destination = SAMLRequest.Descendants(XName.Get("AuthnRequest", SAMLServiceOptions.SamlpNamespace)).Single().Attribute("AssertionConsumerServiceURL").Value;

			log.LogInformation("Creating SAMLResponse. InResponseTo = {inResponseTo}, Audience = {audience}, Destination = {destination}, Issuer = {issuer}",
				inResponseTo, audience, destination, options.ResponseIssuer);

			SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
			{
				Audience = audience,
				Issuer = options.ResponseIssuer,
				Subject = new ClaimsIdentity(principal.Identity)
			};
			Saml2SecurityToken securityToken = PrepareSecurityToken(inResponseTo, destination, tokenDescriptor);
			log.LogInformation("Saml2SecurityToken created");

			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
				{
					await WriteSamlResponse(inResponseTo, destination, securityToken, xmlWriter);
				}
			}


			XDocument samlResponse = XDocument.Parse(sb.ToString());
			log.LogDebug("SAMLResponse = {samlResponse}", sb.ToString());

			log.LogInformation("XDocument with SAMLResponse created");
			if (sign)
			{
				return await SignSAMLResponse(samlResponse, cancellationToken);
			}

			return samlResponse;
		}

		/// <summary>
		/// Get decoded and inflated SAML request
		/// </summary>
		/// <param name="SAMLRequest">base 64 SAML request</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Parsed Xdocuent of SAML request</returns>
		public async Task<XDocument> GetDecodedInflatedSAMLRequest(string SAMLRequest, CancellationToken cancellationToken = default)
		{

			string decoded;
			using (var input = new MemoryStream(Convert.FromBase64String(SAMLRequest)))
			{
				log.LogInformation("Decoded SAMLRequest from base64");
				using (var unzip = new DeflateStream(input, CompressionMode.Decompress))
				{
					using (var reader = new StreamReader(unzip, Encoding.UTF8))
					{
						log.LogInformation("Inflating SAMLRequest");
						decoded = await reader.ReadToEndAsync();
						log.LogInformation("SAMLRequest Decoded and Inflated");
						log.LogDebug("Decoded and Inflated SAMLRequest = {samlRequest}", decoded);
					}
				}
			}
			return XDocument.Parse(decoded);
		}

		/// <summary>
		/// Create XDocument metadata
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<XDocument> GetMetadata(CancellationToken cancellationToken = default)
		{
			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
				{
					await xmlWriter.WriteStartDocumentAsync();
					await xmlWriter.WriteStartElementAsync("md", "EntityDescriptor", "urn:oasis:names:tc:SAML:2.0:metadata");
					await xmlWriter.WriteAttributeStringAsync(null, "entityID", null, options.ResponseIssuer);
					await xmlWriter.WriteStartElementAsync("md", "IDPSSODescriptor", null);
					await xmlWriter.WriteAttributeStringAsync(null, "WantAuthnRequestsSigned", null, "false");
					await xmlWriter.WriteAttributeStringAsync(null, "protocolSupportEnumeration", null, SAMLServiceOptions.SamlpNamespace);
					await xmlWriter.WriteStartElementAsync("md", "KeyDescriptor", null);
					await xmlWriter.WriteAttributeStringAsync(null, "use", null, "signing");
					await xmlWriter.WriteStartElementAsync("ds", "KeyInfo", "http://www.w3.org/2000/09/xmldsig#");
					await xmlWriter.WriteStartElementAsync("ds", "X509Data", null);
					await xmlWriter.WriteElementStringAsync("ds", "X509Certificate", null, Convert.ToBase64String(options.SigningCertificate.Export(X509ContentType.Cert)));
					await xmlWriter.WriteEndElementAsync();
					await xmlWriter.WriteEndElementAsync();
					await xmlWriter.WriteEndElementAsync();
					await xmlWriter.WriteStartElementAsync("md", "SingleSignOnService", null);
					await xmlWriter.WriteAttributeStringAsync(null, "Binding", null, "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect");
					await xmlWriter.WriteAttributeStringAsync(null, "Location", null, options.SSOLocation);
					await xmlWriter.WriteEndElementAsync();
					await xmlWriter.WriteEndElementAsync();
					await xmlWriter.WriteEndElementAsync();
					await xmlWriter.WriteEndDocumentAsync();
				}
			}

			return XDocument.Parse(sb.ToString());
		}

		/// <summary>
		/// Sign SAML response 
		/// </summary>
		/// <param name="SAMLResponse">XDocument document object</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public Task<XDocument> SignSAMLResponse(XDocument SAMLResponse, CancellationToken cancellationToken = default)
		{
			log.LogInformation("Starting signing SAMLResponse");
			log.LogInformation("Reading XDocument into XmlDocument");

			var xmlDocument = new XmlDocument() { PreserveWhitespace = true };
			using (var xmlReader = SAMLResponse.CreateReader())
			{
				xmlDocument.Load(xmlReader);
			}
			log.LogInformation("Preparing SignedXml from XmlDocument");
			SignedXml signedXml = PrepareSignedXml(SAMLResponse, xmlDocument);
			log.LogInformation("Computing Signature");
			signedXml.ComputeSignature();
			var signature = signedXml.GetXml();

			var clonedResponse = new XDocument(SAMLResponse);

			log.LogInformation("Prepending the computed Signature to the Status node");
			// Append the computed signature. The signature must be placed as the sibling of the Issuer element.
			var statusNode = clonedResponse.Descendants(XName.Get("Status", SAMLServiceOptions.SamlpNamespace)).FirstOrDefault();
			statusNode.AddBeforeSelf(XElement.Parse(signature.OuterXml));
			log.LogInformation("Signature prepended");
			log.LogDebug("Signed SAMLResponse = {signedSamlResponse}", clonedResponse.ToString());

			return Task.FromResult(clonedResponse);
		}

		#region Private Methods
		private Saml2SecurityToken PrepareSecurityToken(string inResponseTo, string destination, SecurityTokenDescriptor tokenDescriptor)
		{
			Saml2SecurityToken securityToken = (Saml2SecurityToken)samlTokenHandler.CreateToken(tokenDescriptor);

			Saml2AuthenticationContext authContext = new Saml2AuthenticationContext(new Uri(SAMLServiceOptions.TLSClientAuthenticationMethodUri));
			Saml2AuthenticationStatement authStatement = new Saml2AuthenticationStatement(authContext);

			Saml2SubjectConfirmationData subConfirmationData = new Saml2SubjectConfirmationData()
			{
				InResponseTo = new Saml2Id(inResponseTo),
				Recipient = new Uri(destination),
				NotBefore = DateTime.UtcNow,
				NotOnOrAfter = DateTime.UtcNow.AddMinutes(60)
			};
			Saml2SubjectConfirmation subConfirmation = new Saml2SubjectConfirmation(Saml2Constants.ConfirmationMethods.Bearer, subConfirmationData);
			securityToken.Assertion.Statements.Add(authStatement);
			securityToken.Assertion.Subject.SubjectConfirmations.Add(subConfirmation);
			return securityToken;
		}
		private SignedXml PrepareSignedXml(XDocument SAMLResponse, XmlDocument xmlDocument)
		{
			SignedXml signedXml = new SignedXml(xmlDocument);
			signedXml.SigningKey = options.SigningCertificate.PrivateKey;
			signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
			signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

			Reference reference = new Reference($"#{SAMLResponse.Root.Attribute("ID")?.Value}");
			reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
			reference.AddTransform(new XmlDsigExcC14NTransform());
			reference.DigestMethod = SignedXml.XmlDsigSHA256Url;

			signedXml.AddReference(reference);
			KeyInfo keyInfo = new KeyInfo();
			KeyInfoX509Data keyInfoData = new KeyInfoX509Data(options.SigningCertificate);
			keyInfo.AddClause(keyInfoData);

			signedXml.KeyInfo = keyInfo;

			return signedXml;
		}

		private async Task WriteSamlResponse(string inResponseTo, string destination, Saml2SecurityToken securityToken, XmlWriter xmlWriter)
		{
			await xmlWriter.WriteStartElementAsync("samlp", "Response", SAMLServiceOptions.SamlpNamespace); //opens Response
			await xmlWriter.WriteAttributeStringAsync("xmlns", "saml", null, SAMLServiceOptions.SamlNamespace); //declares saml namespace
			await xmlWriter.WriteAttributeStringAsync(null, Saml2Constants.Attributes.ID, null, $"_{Guid.NewGuid()}"); //writes ID attribute
			await xmlWriter.WriteAttributeStringAsync(null, Saml2Constants.Attributes.Version, null, Saml2Constants.Version); //writes Version attribute
			await xmlWriter.WriteAttributeStringAsync(null, Saml2Constants.Attributes.IssueInstant, null, DateTime.UtcNow.ToString("o")); //writes IssueInstant attribute
			await xmlWriter.WriteAttributeStringAsync(null, "Destination", null, destination); //writes Destination attribute
			await xmlWriter.WriteAttributeStringAsync(null, Saml2Constants.Attributes.InResponseTo, null, inResponseTo); //writes InResponseTo attributes

			await xmlWriter.WriteStartElementAsync("saml", Saml2Constants.Elements.Issuer, null); //opens Issuer
			await xmlWriter.WriteStringAsync(options.ResponseIssuer); //wirtes the issuer value
			await xmlWriter.WriteEndElementAsync(); //closes Issuer

			await xmlWriter.WriteStartElementAsync("samlp", "Status", null); //opens Status
			await xmlWriter.WriteStartElementAsync("samlp", "StatusCode", null); //opens StatusCode
			await xmlWriter.WriteAttributeStringAsync(null, "Value", null, SAMLServiceOptions.StatusCodeSuccess); //adds Value to StatusCode
			await xmlWriter.WriteEndElementAsync(); //closes StatusCode
			await xmlWriter.WriteEndElementAsync(); //closes Status

			samlTokenHandler.WriteToken(xmlWriter, securityToken); //writes Assertion

			await xmlWriter.WriteEndElementAsync(); // closes Response
		}
		#endregion

	}

	public class SAMLServiceOptions
	{
		public const string TLSClientAuthenticationMethodUri = " urn:oasis:names:tc:SAML:2.0:ac:classes:TLSClient";
		public const string SamlNamespace = "urn:oasis:names:tc:SAML:2.0:assertion";
		public const string SamlpNamespace = "urn:oasis:names:tc:SAML:2.0:protocol";
		public const string StatusCodeSuccess = "urn:oasis:names:tc:SAML:2.0:status:Success";
		public X509Certificate2 SigningCertificate { get; set; }
		public string ResponseIssuer { get; set; }
		public string SSOLocation { get; set; }
	}
}
