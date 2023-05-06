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

    public SAMLService(ICertificateService certificateService,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SAMLService> logger)
    {
        _certificateService = certificateService;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
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
                HttpUtility.ParseQueryString(relayState.Substring(questMarkIndex)) : null;
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

            wctxQueryString = questMarkIndex > -1 ? HttpUtility.ParseQueryString(wctx.Substring(questMarkIndex)) : null;

        }

        return wctxQueryString;
    }

    public bool HasReferrer()
    {
        return _httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Referer");
    }
}