/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Specialized;

namespace Microsoft.SPID.Proxy.Services.Implementations;

public class FederatorRequestService : IFederatorRequestService
{
    private readonly ISPIDService _spidService;
    private readonly ISAMLService _samlService;
    private readonly IIDPService _idpService;
    private readonly ILogger _logger;
    private readonly FederatorOptions _federatorOptions;
    private readonly SPIDOptions _spidOptions;
    private readonly AttributeConsumingServiceOptions _attributeConsumingServiceOptions;

    public FederatorRequestService(ISPIDService spidService,
        ISAMLService samlService,
        IIDPService idpService,
        ILogger<FederatorRequestService> logger,
        IOptions<FederatorOptions> federatorOptions,
        IOptions<SPIDOptions> spidOptions,
        IOptions<AttributeConsumingServiceOptions> attributeConsumingServiceOption,
        IOptions<IDPMetadatasOptions> idpMetadatasOptions)
    {
        _spidService = spidService;
        _samlService = samlService;
        _idpService = idpService;
        _logger = logger;
        _federatorOptions = federatorOptions.Value;
        _spidOptions = spidOptions.Value;
        _attributeConsumingServiceOptions = attributeConsumingServiceOption.Value;
    }

    public async Task<string> GetRedirectUriAsync(FederatorRequest federatorRequest, XmlDocument requestAsXml)
    {
        var outcomingSAMLRequest = SetOutcomingXml(federatorRequest, requestAsXml)
            .InnerXml
            .EncodeSamlRequest();

        var outcomingSignature = await _samlService.GetSignature(federatorRequest, outcomingSAMLRequest, federatorRequest.SigAlg, federatorRequest.RelayState);

        var idenityProviderUrl = _idpService.GetIDPUrl(federatorRequest.IdentityProvider);

        string redirectUrl;

        if (!requestAsXml.SAMLRequestIsLogout())
            redirectUrl = $"{idenityProviderUrl}?SAMLRequest={outcomingSAMLRequest}&RelayState={federatorRequest.RelayState}&SigAlg={federatorRequest.SigAlg}&Signature={outcomingSignature}";
        else
            redirectUrl = $"{idenityProviderUrl}?SAMLRequest={outcomingSAMLRequest}&SigAlg={federatorRequest.SigAlg}&Signature={outcomingSignature}";

        _logger.LogInformation(LoggingEvents.REDIRECT_URL_CREATED,"Redirect Url: {redirectUrl}", redirectUrl);

        return redirectUrl;
    }

    public string GetPassThrowRedirectUri(FederatorRequest federatorRequest)
    {
        var idenityProviderUrl = _idpService.GetIDPUrl(federatorRequest.IdentityProvider);

        return $"{idenityProviderUrl}?SAMLRequest={federatorRequest.SAMLRequest}&RelayState={federatorRequest.RelayState}&SigAlg={federatorRequest.SigAlg}&Signature={federatorRequest.Signature}";
    }

    private XmlDocument SetOutcomingXml(FederatorRequest federatorRequest, XmlDocument requestAsXml)
    {
        var outcomingSAMLXml = !requestAsXml.SAMLRequestIsLogout() ?
            SetOutcomingSAMLXmlSignIn(federatorRequest, requestAsXml) :
            SetOutcomingSAMLXmlLogout(federatorRequest, requestAsXml);

        _logger.LogInformation(LoggingEvents.OUTGOING_SAML_REQUEST_CREATED,"Outgoing SAMLRequest: {outgoingSamlRequest}", outcomingSAMLXml.OuterXml);

        return outcomingSAMLXml;
    }

    private XmlDocument SetOutcomingSAMLXmlLogout(FederatorRequest federatorRequest, XmlDocument requestAsXml)
    {
        try
        {
            XmlElement rootEl = requestAsXml.DocumentElement;

            if (rootEl != null)
            {
                var idenityProviderUrl = _idpService.GetIDPUrl(federatorRequest.IdentityProvider);
                rootEl.SetAttribute("Destination", idenityProviderUrl);
                rootEl.RemoveAttribute("Consent");

				string entityId = GetNewEntityId(federatorRequest);
				requestAsXml.ChangeIssuer(entityId);
            }
            return requestAsXml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Something went wrong transforming the incoming SAML Xml into the desired outgoing SAML Xml.");
            throw;
        }
    }

    private XmlDocument SetOutcomingSAMLXmlSignIn(FederatorRequest federatorRequest, XmlDocument requestAsXml)
    {
        XmlElement rootEl = requestAsXml.DocumentElement;

        if (rootEl == null)
        {
            throw new InvalidOperationException("Root element of XMLDocument is null");
        }

        NameValueCollection refererQueryString = null, relayQueryString = null, wctxQueryString = null;

        //gets querystrings from Referrer itself, relaystate (inside referrer), wctx (inside referrer)
        if (_samlService.HasReferrer())
        {
            refererQueryString = _samlService.GetRefererQueryString();
            relayQueryString = _samlService.GetRelayStateQueryString(refererQueryString);
            wctxQueryString = _samlService.GetWCTXQueryString(refererQueryString);
        }

        _logger.LogDebug("RefererQueryString: {referer}", refererQueryString?.ToString());
        _logger.LogDebug("RelayState QueryString: {relayState}", relayQueryString?.ToString());
        _logger.LogDebug("Wctx QueryString: {wctx}", wctxQueryString?.ToString());

        // Add root namespaces here
        var nameSpaceMgr = new XmlNamespaceManager(requestAsXml.NameTable);
        var samlpProtocolNamespace = "urn:oasis:names:tc:SAML:2.0:protocol";
        nameSpaceMgr.AddNamespace("samlp", samlpProtocolNamespace);

        try
        {
            var attributeConsumingService = federatorRequest.GetAttributeConsumingService(
                _attributeConsumingServiceOptions.CIEAttributeConsumingService,
                _attributeConsumingServiceOptions.EIDASAttributeConsumingService,
                _spidService.GetACSValue(refererQueryString, relayQueryString, wctxQueryString)
            );
            requestAsXml.SetAttributeConsumingService(attributeConsumingService);

            var idenityProviderUrl = _idpService.GetIDPUrl(federatorRequest.IdentityProvider);
            rootEl.SetAttribute("Destination", idenityProviderUrl);
            rootEl.RemoveAttribute("Consent");
            rootEl.RemoveAttribute("IsPassive");
            string entityId = GetNewEntityId(federatorRequest);
            requestAsXml.ChangeIssuer(entityId);

            var spidL = _spidService.GetSPIDLValue(refererQueryString, relayQueryString, wctxQueryString);
            //If no RequestedAuthnContext is already present, add it
            if (requestAsXml.GetElementsByTagName("RequestedAuthnContext", "*").Count == 0)
            {
                requestAsXml.AddRequestedAuthnContext(string.Format(_spidOptions.SPIDLUri, spidL));
            }
            //RequestedAuthnContext is already present, remove uncompliant values
            else
            {
                requestAsXml.RemoveUncompliantAuthnContextClassrefs();
            }

            _logger.LogDebug("Adding ForceAuthn attribute = 'true' due to SPIDL > 1");
            requestAsXml.SetForceAuthn()
                .SetComparison()
                .SetAuthnContextClassRefIfNotPresent(string.Format(_spidOptions.SPIDLUri, spidL));

            if (_attributeConsumingServiceOptions.UpdateAssertionConsumerServiceUrl)
            {
                var ACSU = rootEl.GetAttributeNode("AssertionConsumerServiceURL");
                ACSU.Value = _samlService.GetAttributeConsumerService();
            }

            // If no Extensions/Purpose is already present, add it
            string purposeValue = _spidService.GetPurposeValue(refererQueryString, relayQueryString, wctxQueryString);

            if (!string.IsNullOrWhiteSpace(purposeValue))
            {
                requestAsXml.AddExtensionsAndPurposeIfNotPresent(nameSpaceMgr, samlpProtocolNamespace, purposeValue);
            }

            return requestAsXml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Something went wrong transforming the incoming SAML Xml into the desired outgoing SAML Xml");
            throw;
        }
    }

    private string GetNewEntityId(FederatorRequest federatorRequest)
    {
        return federatorRequest.IsCIE() ? _federatorOptions.CIEEntityId : _federatorOptions.SPIDEntityId;
    }
}