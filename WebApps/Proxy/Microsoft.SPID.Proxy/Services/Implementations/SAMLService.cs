/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.SPID.Proxy.Services.Implementations;

public class SAMLService : ISAMLService
{
    private readonly ICertificateService _certificateService;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FederatorOptions _federatorOptions;
    private readonly IDistributedCache _cache;

    public SAMLService(ICertificateService certificateService,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SAMLService> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<FederatorOptions> federatorOptions,
        IDistributedCache cache)
    {
        _certificateService = certificateService;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _federatorOptions = federatorOptions.Value;
        _cache = cache;
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

    /// <summary>
    /// Validates the signature of an incoming SAMLRequest from the Federator.
    /// </summary>
    /// <param name="federatorRequest">The FederatorRequest containing the SAMLRequest and signature parameters.</param>
    /// <returns>True if the signature is valid; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Federator MetadataUrl is not configured.</exception>
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

            // Validate SigAlg parameter
            if (string.IsNullOrEmpty(federatorRequest.SigAlg))
            {
                _logger.LogWarning("SAMLRequest SigAlg parameter is missing");
                return false;
            }

            // Decode and validate that SigAlg matches expected algorithm
            string decodedSigAlg = HttpUtility.UrlDecode(federatorRequest.SigAlg);
            if (!decodedSigAlg.Equals("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("SAMLRequest SigAlg '{sigAlg}' is not supported. Expected rsa-sha256", decodedSigAlg);
                return false;
            }

            // Get Federator metadata to extract public certificate
            if (string.IsNullOrWhiteSpace(_federatorOptions.MetadataUrl))
            {
                _logger.LogError("Federator MetadataUrl is not configured");
                throw new InvalidOperationException("Federator MetadataUrl is not configured");
            }

            // Try to get metadata from cache first
            string metadataXml = null;
            string cacheKey = "FederatorMetadata";
            bool fetchedFromHttp = false;

            if (_cache != null)
            {
                var metadataXmlFromCache = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrWhiteSpace(metadataXmlFromCache))
                {
                    _logger.LogDebug("Federator metadata retrieved from cache");
                    metadataXml = metadataXmlFromCache;
                }
            }

            // If not in cache, fetch from HTTP
            if (_cache == null || string.IsNullOrWhiteSpace(metadataXml))
            {
                try
                {
                    using var httpClient = _httpClientFactory.CreateClient("default");
                    metadataXml = await httpClient.GetStringAsync(_federatorOptions.MetadataUrl);
                    fetchedFromHttp = true;
                    _logger.LogDebug("Federator metadata fetched from {metadataUrl}", _federatorOptions.MetadataUrl);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Failed to fetch Federator metadata from {metadataUrl}", _federatorOptions.MetadataUrl);
                    return false;
                }
            }

            XmlDocument metadataDocument = new XmlDocument();
            metadataDocument.PreserveWhitespace = true;
            
            try
            {
                metadataDocument.LoadXml(metadataXml);
            }
            catch (XmlException ex)
            {
                _logger.LogError(ex, "Failed to parse Federator metadata XML");
                return false;
            }

            // Cache metadata if it was fetched from HTTP
            if (_cache != null && fetchedFromHttp)
            {
                var options = new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(120),
                    SlidingExpiration = null
                };
                _logger.LogDebug("Storing Federator metadata in cache");
                await _cache.SetStringAsync(cacheKey, metadataXml, options);
            }

            // Extract certificates from metadata
            var certificates = metadataDocument.GetCertificates();
            
            if (certificates == null || certificates.Count == 0)
            {
                _logger.LogError("No certificates found in Federator metadata");
                return false;
            }

            // Reconstruct the signed string - must match the order used when signing
            // Note: RelayState and SigAlg in FederatorRequest are already UpperCaseUrlEncoded
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
        catch (InvalidOperationException)
        {
            // Re-throw configuration errors
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating SAMLRequest signature");
            return false;
        }
    }
}