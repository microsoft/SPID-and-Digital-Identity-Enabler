/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPID.Proxy.Services.Implementations;

public class FederatorResponseService : IFederatorResponseService
{
    private readonly IXMLResponseCheckService _xmlResponseCheckService;
    private readonly ICertificateService _certificateService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDistributedCache _cache;
    private readonly ILogger _logger;
    private readonly IDPMetadatasOptions _idpMetadatasOptions;
    private readonly CustomErrorOptions _customErrorOptions;
    private readonly TechnicalChecksOptions _technicalChecksOptions;
    private readonly FederatorOptions _federatorOptions;

    public FederatorResponseService(IXMLResponseCheckService xmlResponseCheckService,
        ICertificateService certificateService,
        IHttpClientFactory httpClientFactory,
        IDistributedCache cache,
        ILogger<FederatorResponseService> logger,
        IOptions<CustomErrorOptions> customErrorOptions,
        IOptions<IDPMetadatasOptions> idpMetadataOptions,
        IOptions<TechnicalChecksOptions> technicalChecksOptions,
        IOptions<FederatorOptions> federatorOptions)
    {
        _xmlResponseCheckService = xmlResponseCheckService;
        _certificateService = certificateService;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
        _customErrorOptions = customErrorOptions.Value;
        _idpMetadatasOptions = idpMetadataOptions.Value;
        _technicalChecksOptions = technicalChecksOptions.Value;
        _federatorOptions = federatorOptions.Value;
    }

    public FederatorResponse GetFederatorResponse(XmlDocument samlResponse, string relayState)
    {
        return new(samlResponse.EncodeSamlResponse(), relayState, _federatorOptions.FederatorAttributeConsumerServiceUrl);
    }

    public void RunTechnicalChecks(XmlDocument responseXml)
    {
        if (!_technicalChecksOptions.SkipTechnicalChecks)
        {
            _xmlResponseCheckService.CheckResponseVersion(responseXml);
            _logger.LogTrace("Checked Response Version");

            _xmlResponseCheckService.CheckResponseIssueInstant(responseXml);
            _logger.LogTrace("Checked Response IssueInstant");

            _xmlResponseCheckService.CheckResponseInResponseTo(responseXml);
            _logger.LogTrace("Checked Response InResponseTo");

            _xmlResponseCheckService.CheckAuthnContextClassRef(responseXml);
            _logger.LogTrace("Checked AuthnContextClassRef");

            _xmlResponseCheckService.CheckResponseIssuer(responseXml);
            _logger.LogTrace("Checked Response Issuer");

            _xmlResponseCheckService.CheckNameID(responseXml);
            _logger.LogTrace("Checked NameID");

            _xmlResponseCheckService.CheckSubjectConfirmation(responseXml);
            _logger.LogTrace("Checked SubjectConfirmation");

            _xmlResponseCheckService.CheckSubjectConfirmationData(responseXml);
            _logger.LogTrace("Checked SubjectConfirmationData");

            _xmlResponseCheckService.CheckAssertion(responseXml);
            _logger.LogTrace("Checked Assertion");

            _xmlResponseCheckService.CheckConditions(responseXml);
            _logger.LogTrace("Checked Conditions");

            _xmlResponseCheckService.CheckAttributes(responseXml);
            _logger.LogTrace("Checked Attributes");
        }
    }

    public async Task<bool> CheckSignature(XmlDocument responseXml)
    {
        var fullIssuer = responseXml.GetElementsByTagName("Issuer", "*")[0].InnerText;
        string unprefixedIssuer = fullIssuer;
		foreach (var prefix in _idpMetadatasOptions.MetadataKeyPrefixes)
		{
            unprefixedIssuer = unprefixedIssuer.Replace(prefix, string.Empty);
		}

        var metadataUrl = _idpMetadatasOptions.MetadataMapping.GetValueOrDefault(unprefixedIssuer);

        if (string.IsNullOrWhiteSpace(metadataUrl))
            throw new SPIDValidationException($"{fullIssuer} is an unknown issuer");

        XmlDocument metadataDocument = new XmlDocument();
        string metadataXml = null;
        string cacheKey = $"Metadata_{fullIssuer}";

        if (_cache != null)
        {
            var metadataXmlFromCache = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrWhiteSpace(metadataXmlFromCache))
            {
                _logger.LogDebug("Metadata for issuer {issuer} retrieved from cache", fullIssuer);
                metadataXml = metadataXmlFromCache;
            }
        }

        if (_cache == null || string.IsNullOrWhiteSpace(metadataXml))
        {
            using var httpClient = _httpClientFactory.CreateClient();
            metadataXml = await httpClient.GetStringAsync(metadataUrl);
            if (_cache != null)
            {
                var options = new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_idpMetadatasOptions.CacheAbsoluteExpirationInMins),
                    SlidingExpiration = null
                };
                _logger.LogDebug("Storing metadata for issuer {issuer} in cache", fullIssuer);
                await _cache.SetStringAsync(cacheKey, metadataXml, options);
            }
        }


        metadataDocument.LoadXml(metadataXml);

        var certificates = metadataDocument.GetCertificates();

        if (!responseXml.IsSigned() || !responseXml.CheckSignature())
            return false;

        bool validated = false;
        foreach (var cert in certificates)
        {
            if (responseXml.CheckSignature(cert.GetRSAPublicKey()))
            {
                validated = true;
                break;
            }
        }

        if (!validated)
            return false;

        if (!_technicalChecksOptions.SkipAssertionSignatureValidation)
        {
            var assertion = responseXml.GetElementsByTagName("Assertion", "*")?[0];
            if (assertion != null)
            {
                XmlDocument assertionDoc = new XmlDocument();
                assertionDoc.PreserveWhitespace = true;
                assertionDoc.LoadXml(assertion.OuterXml);
                if (!assertionDoc.IsSigned() || !assertionDoc.CheckSignature())
                    return false;
                validated = false;

                foreach (var cert in certificates)
                {
                    if (assertionDoc.CheckSignature(cert.GetRSAPublicKey()))
                    {
                        validated = true;
                        break;
                    }
                }
                if (!validated)
                    return false;
            }
        }

        return true;
    }

    public bool ResponseHasBlockingStatusCode(XmlDocument responseXml, out SPIDErrorModel errorModel)
    {
        errorModel = null;

        var statusCode = responseXml.GetElementsByTagName("StatusCode", "*")[0];
        var statusCodeValue = statusCode.Attributes["Value"].Value;
        if (statusCodeValue.Equals(Saml20Constants.StatusCodes.Success))
            return false;

        _logger.LogError("StatusCode not Success. StatusCode = {responseStatusCode}", statusCodeValue);

        if (string.IsNullOrWhiteSpace(statusCodeValue))
            throw new SPIDValidationException("StatusCode empty");

        var statusMessage = responseXml.GetElementsByTagName("StatusMessage", "*")?[0];

        errorModel = new SPIDErrorModel()
        {
            StatusCode = new HtmlString(statusCodeValue),
            StatusMessage = new HtmlString(statusMessage?.InnerText),
            UserFriendlyMessage = new HtmlString(GetUserFriendlyMessage(statusMessage?.InnerText))
        };
        return true;
    }

    public string GetUserFriendlyMessage(string statusMessageText)
    {
        if (string.IsNullOrWhiteSpace(statusMessageText))
            return string.Empty;

        string[] customErrors = new string[] { "19", "20", "21", "22", "23", "25" };

        foreach (var ce in customErrors)
        {
            if (statusMessageText.Contains($"nr{ce}"))
                return _customErrorOptions.Values[$"ErrorCode{ce}Message"];
        }

        return string.Empty;
    }

    public async Task SignWholeResponseMessageAsync(XmlDocument doc, string responseDigestMethod)
    {
        var responseId = doc.DocumentElement.Attributes["ID"].Value;
        var cert = await _certificateService.GetProxySignCertificate();
        var signedXml = doc.SignDocument(responseId, cert, responseDigestMethod, _federatorOptions.X509IncludeOption);

        // Append the computed signature. The signature must be placed as the sibling of the Issuer element.
        XmlNodeList nodes = doc.DocumentElement.GetElementsByTagName("Status", Saml20Constants.PROTOCOL);
        nodes[0].ParentNode.InsertBefore(doc.ImportNode(signedXml.GetXml(), true), nodes[0]);
    }

    public async Task SignAssertionAsync(XmlDocument doc, string assertionDigestMethod)
    {
        var assertionId = doc.GetElementsByTagName("Assertion", Saml20Constants.ASSERTION)[0].Attributes["ID"].Value;
        var cert = await _certificateService.GetProxySignCertificate();
        var signedXml = doc.SignDocument(assertionId, cert, assertionDigestMethod, _federatorOptions.X509IncludeOption);

        // Append the computed signature. The signature must be placed as the sibling of the Issuer element.
        XmlNodeList nodes = doc.DocumentElement.GetElementsByTagName("Issuer", Saml20Constants.ASSERTION);
        XmlNode assertionNode = nodes[nodes.Count - 1];
        // may return 2 nodes: Issuer of the response and issuer of the assertion        
        assertionNode.ParentNode.InsertAfter(doc.ImportNode(signedXml.GetXml(), true), assertionNode);
    }
}