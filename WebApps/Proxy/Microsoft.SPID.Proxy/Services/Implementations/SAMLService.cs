/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace Microsoft.SPID.Proxy.Services.Implementations;

public class SAMLService : ISAMLService
{
    private readonly ICertificateService _certificateService;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FederatorOptions _federatorOptions;

    public SAMLService(ICertificateService certificateService,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SAMLService> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<FederatorOptions> federatorOptions)
    {
        _certificateService = certificateService;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _federatorOptions = federatorOptions.Value;
    }

    public string GetAttributeConsumerService()
    {
        return $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/proxy/assertionconsumer";

    }

    public NameValueCollection GetRefererQueryString()
    {
        Uri refererUri = new Uri(_httpContextAccessor.HttpContext.Request.Headers["Referer"]);
        return HttpUtility.ParseQueryString(refererUri.Query);
    }

    public NameValueCollection GetRelayStateQueryString(NameValueCollection refererQueryString)
    {
        NameValueCollection relayQueryString = null;
        var relayState = refererQueryString["RelayState"];
        if (relayState != null)
        {
            var questMarkIndex = relayState.IndexOf("?");

            relayQueryString = questMarkIndex > -1 ?
                HttpUtility.ParseQueryString(relayState.Substring(questMarkIndex)) : HttpUtility.ParseQueryString(relayState);
        }

        return relayQueryString;
    }

    public async Task<string> GetSignature(FederatorRequest federatorRequest, string samlRequest, string sigAlg, string relayState)
    {
        try
        {
            var cert = await _certificateService.GetProxySignCertificate();

            using (RSA rsa = cert.GetRSAPrivateKey())
            {
                string stringToBeSigned = string.IsNullOrEmpty(relayState) ? string.Format("SAMLRequest={0}&SigAlg={1}", samlRequest, sigAlg) : string.Format("SAMLRequest={0}&RelayState={1}&SigAlg={2}", samlRequest, relayState, sigAlg);
                var bytesToBeSigned = Encoding.ASCII.GetBytes(stringToBeSigned);
                var signatureBytes = rsa.SignData(bytesToBeSigned, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                _logger.LogDebug("Signature base64String {signature}", Convert.ToBase64String(signatureBytes));
                return HttpUtility.UrlEncode(Convert.ToBase64String(signatureBytes));
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Something went wrong processing the final signature");
            throw;
        }
    }

    public NameValueCollection GetWCTXQueryString(NameValueCollection refererQueryString)
    {
        NameValueCollection wctxQueryString = null;
        var wctx = refererQueryString["wctx"];
        if (wctx != null)
        {
            var questMarkIndex = wctx.IndexOf("?");

            wctxQueryString = questMarkIndex > -1 ? HttpUtility.ParseQueryString(wctx.Substring(questMarkIndex)) 
                : HttpUtility.ParseQueryString(wctx);

        }

        return wctxQueryString;
    }

    public bool HasReferrer()
    {
        return _httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Referer");
    }

    public async Task<bool> ValidateFederatorRequestSignature(FederatorRequest federatorRequest)
    {
        try
        {
            // If signature is missing, return false
            if (string.IsNullOrEmpty(federatorRequest.Signature))
            {
                _logger.LogWarning("SAMLRequest signature is missing");
                return false;
            }

            // Get Federator metadata to extract public certificate
            if (string.IsNullOrWhiteSpace(_federatorOptions.MetadataUrl))
            {
                _logger.LogError("Federator MetadataUrl is not configured");
                throw new InvalidOperationException("Federator MetadataUrl is not configured");
            }

            using var httpClient = _httpClientFactory.CreateClient("default");
            string metadataXml = await httpClient.GetStringAsync(_federatorOptions.MetadataUrl);
            
            XmlDocument metadataDocument = new XmlDocument();
            metadataDocument.LoadXml(metadataXml);

            // Extract certificates from metadata
            var certificates = metadataDocument.GetCertificates();
            
            if (certificates == null || certificates.Count == 0)
            {
                _logger.LogError("No certificates found in Federator metadata");
                return false;
            }

            // Reconstruct the signed string - must match the order used when signing
            string stringToVerify = string.IsNullOrEmpty(federatorRequest.RelayState)
                ? $"SAMLRequest={federatorRequest.SAMLRequest}&SigAlg={federatorRequest.SigAlg}"
                : $"SAMLRequest={federatorRequest.SAMLRequest}&RelayState={federatorRequest.RelayState}&SigAlg={federatorRequest.SigAlg}";

            var bytesToVerify = Encoding.ASCII.GetBytes(stringToVerify);
            var signatureBytes = Convert.FromBase64String(HttpUtility.UrlDecode(federatorRequest.Signature));

            // Try to verify signature with each certificate from Federator metadata
            foreach (var cert in certificates)
            {
                using (RSA rsa = cert.GetRSAPublicKey())
                {
                    if (rsa != null && rsa.VerifyData(bytesToVerify, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                    {
                        _logger.LogDebug("SAMLRequest signature validated successfully with certificate: {subject}", cert.Subject);
                        return true;
                    }
                }
            }

            _logger.LogWarning("SAMLRequest signature could not be validated with any certificate from Federator metadata");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating SAMLRequest signature");
            return false;
        }
    }
}