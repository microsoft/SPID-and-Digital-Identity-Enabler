/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CNS.Auth.Web.Services
{
	/// <summary>
	/// Implementation of the CNS certificare service
	/// </summary>
	public class CNSCertificateService : ICNSCertificateService
	{
		private readonly ILogger<CNSCertificateService> log;
		private readonly CNSCertificateServiceOptions options;
		private readonly HttpClient _httpClient;

		public CNSCertificateService(ILogger<CNSCertificateService> log, IOptions<CNSCertificateServiceOptions> options, IHttpClientFactory httpClientFactory)
		{
			if (httpClientFactory is null)
			{
				throw new ArgumentNullException(nameof(httpClientFactory));
			}

			this._httpClient = httpClientFactory.CreateClient();
			this.log = log ?? throw new ArgumentNullException(nameof(log));
			this.options = options.Value;
		}

		/// <summary>
		/// Get principla Claims from certificate
		/// </summary>
		/// <param name="cert">X509 Certificate</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Claims Principal for the specified identity </returns>
		public Task<ClaimsPrincipal> GetClaimsPrincipal(X509Certificate2 cert, CancellationToken cancellationToken = default)
		{
			log.LogInformation("Retrieving claims from certificate with thumbprint {thumbprint}", cert.Thumbprint);
			string CN = cert.GetNameInfo(X509NameType.SimpleName, false);
			Claim commonName = new Claim("commonName", CN);
			var subjectRawData = cert.SubjectName.RawData;

			var SN = new AsnEncodedData("SN", subjectRawData).Format(false);
			var G = new AsnEncodedData("G", subjectRawData).Format(false);
			var OU = new AsnEncodedData("OU", subjectRawData).Format(false);
			var O = new AsnEncodedData("O", subjectRawData).Format(false);
			var C = new AsnEncodedData("C", subjectRawData).Format(false);


			//TODO Read info from CNS certificate
			Claim givenName = new Claim(ClaimTypes.GivenName, G);
			Claim surname = new Claim(ClaimTypes.Surname, SN);
			Claim organizationUnit = new Claim("organizationUnit", OU);
			Claim organization = new Claim("organization", O);
			Claim country = new Claim("country", C);

			var match = CNSCertificateServiceOptions.CommonNameRegex.Match(CN);

			Claim fiscNumber = new Claim("fiscalNumber", match.Groups["cf"].Value);
			Claim idCard = new Claim("idCard", match.Groups["cardId"].Value);
			Claim hash = new Claim("cnHash", match.Groups["hash"].Value);
			Claim displayName = new Claim(ClaimTypes.Name, $"{G} {SN}");
			Claim nameId = new Claim(ClaimTypes.NameIdentifier, CN);

			log.LogInformation("Claims retrieved successfully. Returning ClaimsPrincipal");

			return Task.FromResult(
				new ClaimsPrincipal(
					new ClaimsIdentity(
						new Claim[] { displayName, givenName, surname, fiscNumber, organization, organizationUnit, country, idCard, hash, commonName, nameId },
						CertificateAuthenticationDefaults.AuthenticationScheme
					)
				)
			);
		}

		/// <summary>
		/// Validate certificate with thumbprint for CNS Certificate policies
		/// </summary>
		/// <param name="cert">X509 Certificate</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns>Bool value for validation</returns>
		public Task<bool> ValidateCNSCertificate(X509Certificate2 cert, CancellationToken cancellationToken = default)
		{
			log.LogInformation("Validating certificate with thumbprint {thumbprint} for CNS Certificate Policies", cert.Thumbprint);

			var certificatePoliciesExtensions = cert.Extensions[CNSCertificateServiceOptions.CertificatePoliciesExtension]; //Certificate Policies extension
			if (certificatePoliciesExtensions == null)
			{
				log.LogError($"The ClientCertificate doesn't have the Certificate Policies extension (oid {CNSCertificateServiceOptions.CertificatePoliciesExtension})");
				return Task.FromResult(false);
			}
			var stringCertificatePolicies = certificatePoliciesExtensions.Format(false);

			if (!options.ValidCertificatePolicies.Any(validPolicy => stringCertificatePolicies.Contains(validPolicy, StringComparison.InvariantCultureIgnoreCase)))
			{
				log.LogError("The ClientCertificate doesn't have a valid Certificate Policy specified. Valid policies: {validPolicies}, found policies: {foundPolicies}",
					string.Join(";", options.ValidCertificatePolicies),
					stringCertificatePolicies);
				return Task.FromResult(false);
			}

			log.LogInformation("ClientCertificate has valid Certificate Policies");

			if (options.BlockIfMissingGivenNameOrSurname)
			{
				if (!cert.Subject.Contains("SN=") || !cert.Subject.Contains("G="))
				{
					log.LogError("The ClientCertificate doesn't have SN and/or G in its subject.");
					return Task.FromResult(false);
				}

				log.LogInformation("ClientCertificate Subject has SN and G.");
			}

			if (options.LogSubject)
				log.LogInformation("Certificate Subject: {subject}", cert.Subject);

			return Task.FromResult(true);
		}

		/// <summary>
		/// Get trusted root CS from Trusted list file url
		/// </summary>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns>It Return a X509 Certificate collection</returns>
		public async Task<X509Certificate2Collection> GetTrustedCertificateCollection(CancellationToken cancellationToken = default)
		{
			log.LogInformation("Retrieving Trusted Root CAs from {fileUrl}", options.TrustedListFileUrl);

			string xml = await _httpClient.GetStringAsync(options.TrustedListFileUrl);

			XDocument xDoc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
			XNamespace ns = xDoc.Root.GetDefaultNamespace();

			var servInfo = xDoc.Root
				.Descendants(XName.Get("ServiceInformation", ns.NamespaceName))
				.Where(x => x.Element(XName.Get("ServiceTypeIdentifier", ns.NamespaceName))?.Value == "http://uri.etsi.org/TrstSvc/Svctype/IdV");

			var certificates = servInfo.Descendants(XName.Get("X509Certificate", ns.NamespaceName))
				.Select(x => new X509Certificate2(Convert.FromBase64String(x.Value)));

			return new X509Certificate2Collection(certificates.ToArray());

		}


		/// <summary>
		/// Get certificate authentication options by CertificateAuthenticationOptions object
		/// </summary>
		/// <param name="options">Option used to configure certificate authentication</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns></returns>
		public async Task GetCertificateAuthenticationOptions(CertificateAuthenticationOptions options, CancellationToken cancellationToken = default)
		{
			log.LogInformation("Configuring CertificateAuthenticationOptions instance");
			options.ChainTrustValidationMode = X509ChainTrustMode.CustomRootTrust;
			options.CustomTrustStore = await GetTrustedCertificateCollection();
			options.Events = new CertificateAuthenticationEvents()
			{
				OnCertificateValidated = async ctx =>
				{
					if (!await ValidateCNSCertificate(ctx.ClientCertificate))
					{
						ctx.Fail("Invalid CNS certificate");
						return;
					}

					ctx.Principal = await GetClaimsPrincipal(ctx.ClientCertificate);
					ctx.Success();
					return;
				}
			};

			log.LogInformation("CertificateAuthenticationOptions instance configured");
		}
	}

	public class CNSCertificateServiceOptions
	{
		public const string CNSCertificate = "CNSCertificate";
		public const string CertificatePoliciesExtension = "2.5.29.32";
		public static Regex CommonNameRegex = new Regex(@"^(?<cf>.*)\/(?<cardId>.*)\.(?<hash>.*)$", RegexOptions.Compiled);
		public IEnumerable<string> ValidCertificatePolicies { get; set; }
		public string TrustedListFileUrl { get; set; }
		public int TrustedListPeriodicUpdateHours { get; set; } = 6;
		public bool LogCertificate { get; set; } = true;
		public bool LogSubject { get; set; } = true;
		public bool BlockIfMissingGivenNameOrSurname { get; set; } = false;
	}
}
