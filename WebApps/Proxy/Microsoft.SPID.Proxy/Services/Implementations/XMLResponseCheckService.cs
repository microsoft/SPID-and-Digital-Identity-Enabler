/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Services.Implementations;

public class XMLResponseCheckService : IXMLResponseCheckService
{
    private static readonly string[] cieIssuers = new string[] { "https://idserver.servizicie.interno.gov.it/idp/profile/SAML2/POST/SSO", "https://preproduzione.idserver.servizicie.interno.gov.it/idp/profile/SAML2/POST/SSO" };
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISAMLService _samlService;

    private readonly SPIDOptions _spidOptions;
    private readonly FederatorOptions _federatorOptions;

    public XMLResponseCheckService(IHttpContextAccessor httpContextAccessor,
        ISAMLService samlService,
        IOptions<SPIDOptions> spidOptions,
        IOptions<FederatorOptions> federatorOptions)
    {
        _httpContextAccessor = httpContextAccessor;
		_samlService = samlService;
		_spidOptions = spidOptions.Value;
        _federatorOptions = federatorOptions.Value;
    }

    public void CheckAttributes(XmlDocument responseXml)
    {
        var attributes = responseXml.GetElementsByTagName("Attribute", "*");
        if (attributes == null || attributes.Count == 0)
            throw new SPIDValidationException("No Attribute element present");

        foreach (XmlNode attribute in attributes)
        {
            if (string.IsNullOrWhiteSpace(attribute.InnerText) && !attribute.HasChildNodes)
                throw new SPIDValidationException("Attribute cannot be empty");
        }
    }

    public void CheckSubjectConfirmation(XmlDocument responseXml)
    {
        var subjectConfirmations = responseXml.GetElementsByTagName("SubjectConfirmation", "*");
        if (subjectConfirmations == null || subjectConfirmations.Count < 1)
            throw new SPIDValidationException("SubjectConfirmation not present");

        var subjectConfirmation = subjectConfirmations[0];
        if (subjectConfirmation == null)
            throw new SPIDValidationException("SubjectConfirmation missing");

        var method = subjectConfirmation.Attributes["Method"];
        if (method == null || method.Value != "urn:oasis:names:tc:SAML:2.0:cm:bearer")
            throw new SPIDValidationException("SubjectConfirmation method is missing or different from urn:oasis:names:tc:SAML:2.0:cm:bearer");
    }

    public void CheckResponseInResponseTo(XmlDocument responseXml)
    {
        var inResponseTo = responseXml.DocumentElement.Attributes["InResponseTo"];
        if (inResponseTo == null || string.IsNullOrWhiteSpace(inResponseTo.Value))
            throw new SPIDValidationException("InResponseTo empty or not present");

        if (inResponseTo.Value == "inresponsetodiversodaidrequest")
            throw new SPIDValidationException("InResponseTo different from request id");

    }

    public void CheckResponseIssueInstant(XmlDocument responseXml)
    {
        var issueInstant = responseXml.DocumentElement.Attributes["IssueInstant"];
        if (issueInstant == null || string.IsNullOrWhiteSpace(issueInstant.Value))
            throw new SPIDValidationException("IssueInstant not present or not specified");

        if (!IsValidUTC(issueInstant.Value))
            throw new SPIDValidationException("Response IssueInstant is not UTC");
    }

    public void CheckResponseVersion(XmlDocument responseXml)
    {
        var version = responseXml.DocumentElement.Attributes["Version"];
        if (version == null || version.Value != "2.0")
            throw new SPIDValidationException("Version not present or different from 2.0");
    }

    public void CheckAssertion(XmlDocument responseXml)
    {
        var assertions = responseXml.GetElementsByTagName("Assertion", "*");
        if (assertions == null || assertions.Count < 1)
            throw new SPIDValidationException("Assertion not present");

        var assertion = assertions[0];
        var assertionVersion = assertion.Attributes["Version"];
        if (assertionVersion == null || assertionVersion.Value != "2.0")
            throw new SPIDValidationException("Assertion version must be 2.0");

        var issueInstant = assertion.Attributes["IssueInstant"].Value;
        DateTimeOffset dt = DateTimeOffset.Parse(issueInstant);
        int minutesTolerance = _spidOptions.AssertionIssueInstantToleranceMins;
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        if (dt < utcNow.AddMinutes(-minutesTolerance) || dt > utcNow.AddMinutes(minutesTolerance))
            throw new SPIDValidationException("Assertion IssueInstant too much in the past or in the future");

        var issuers = responseXml.GetElementsByTagName("Issuer", "*");
        if (issuers.Count < 2)
            throw new SPIDValidationException("Assertion Issuer not present");

        var assertionIssuer = issuers[1];
        if (assertionIssuer == null || string.IsNullOrWhiteSpace(assertionIssuer.InnerText))
            throw new SPIDValidationException("Assertion Issuer not specified");

        if (!IsCIE(assertionIssuer.InnerText))
        {
            var assertionIssuerFormat = assertionIssuer.Attributes["Format"];
            if (assertionIssuerFormat == null || string.IsNullOrWhiteSpace(assertionIssuerFormat.Value))
                throw new SPIDValidationException("Assertion Issuer format not specified");

            if (assertionIssuerFormat.Value != Saml20Constants.NameIdentifierFormats.Entity)
                throw new SPIDValidationException($"Assertion Issuer Format must be {Saml20Constants.NameIdentifierFormats.Entity}");
        }
        var responseIssuer = responseXml.GetElementsByTagName("Issuer", "*")[0];
        if (assertionIssuer.InnerText != responseIssuer.InnerText)
            throw new SPIDValidationException("Assertion Issuer and Response Issuer not match");
    }

    public void CheckConditions(XmlDocument responseXml)
    {
        var allConditions = responseXml.GetElementsByTagName("Conditions", "*");
        if (allConditions == null || allConditions.Count < 1)
            throw new SPIDValidationException("Conditions not present");

        var conditions = allConditions[0];
        var notBefore = conditions.Attributes["NotBefore"];
        if (notBefore == null || string.IsNullOrWhiteSpace(notBefore.Value))
            throw new SPIDValidationException("Missing NotBefore on Conditions");

        if (!IsValidUTC(notBefore.Value))
            throw new SPIDValidationException("Conditions NotBefore is not a valid UTC");

        DateTimeOffset notBeforeDt = DateTimeOffset.Parse(notBefore.Value);
        DateTimeOffset responseIssueInstant = DateTimeOffset.Parse(responseXml.DocumentElement.Attributes["IssueInstant"].Value);

        if (notBeforeDt > responseIssueInstant)
            throw new SPIDValidationException("Conditions NotBefore after response IssueInstant");

        var notOnOrAfter = conditions.Attributes["NotOnOrAfter"];
        if (notOnOrAfter == null || string.IsNullOrWhiteSpace(notOnOrAfter.Value))
            throw new SPIDValidationException("Missing NotOnOrAfter on Conditions");

        if (!IsValidUTC(notOnOrAfter.Value))
            throw new SPIDValidationException("Conditions NotOnOrAfter is not a valid UTC");

        DateTimeOffset notOnOrAfterDt = DateTimeOffset.Parse(notOnOrAfter.Value);
        if (notOnOrAfterDt < responseIssueInstant)
            throw new SPIDValidationException("Conditions NotOnOrAfter before response IssueIstant");

        var audiences = responseXml.GetElementsByTagName("Audience", "*");
        if (audiences.Count < 1)
            throw new SPIDValidationException("Audience missing");

        var audience = audiences[0];
        if (audience == null || string.IsNullOrWhiteSpace(audience.InnerText))
            throw new SPIDValidationException("Audience not specified");

        if (audience.InnerText != _federatorOptions.SPIDEntityId)
            throw new SPIDValidationException("Audience is different from SP EntityID");
    }

    public void CheckSubjectConfirmationData(XmlDocument responseXml)
    {
        var subjectConfirmationDatas = responseXml.GetElementsByTagName("SubjectConfirmationData", "*");
        if (subjectConfirmationDatas == null || subjectConfirmationDatas.Count < 1)
            throw new SPIDValidationException("SubjectConfirmationData not present");

        var SubjectConfirmationData = subjectConfirmationDatas[0];
        if (SubjectConfirmationData == null)
            throw new SPIDValidationException("SubjectConfirmationData not present");

        var recipient = SubjectConfirmationData.Attributes["Recipient"]?.Value;
        if (recipient == null)
            return;

        if (recipient != _samlService.GetAttributeConsumerService())
            throw new SPIDValidationException("SubjectConfirmationData Recipient must be equal to AssertionConsumerServiceUrl");

        var inResponseTo = SubjectConfirmationData.Attributes["InResponseTo"];
        if (inResponseTo == null || string.IsNullOrWhiteSpace(inResponseTo.Value))
            throw new SPIDValidationException("SubjectConfirmationData InResponseTo not specified");

        if (inResponseTo.Value == "diversodaauthnrequestid")
            throw new SPIDValidationException("SubjectConfirmationData InResponseTo different from request id");

        var notOnOrAfter = SubjectConfirmationData.Attributes["NotOnOrAfter"];
        if (notOnOrAfter == null || string.IsNullOrWhiteSpace(notOnOrAfter.Value))
            throw new SPIDValidationException("SubjectConfirmationData NotOnOrAfter not specified");

        if (!IsValidUTC(notOnOrAfter.Value))
            throw new SPIDValidationException("SubjectConfirmationData NotOnOrAfter is not UTC");

        DateTimeOffset dtNotOnOrAfter = DateTimeOffset.Parse(notOnOrAfter.Value);
        DateTimeOffset dtIssueInstant = DateTimeOffset.Parse(responseXml.DocumentElement.Attributes["IssueInstant"].Value);

        if (dtNotOnOrAfter < dtIssueInstant)
            throw new SPIDValidationException("SubjectConfirmationData NotOnOrAfter is before than Response IssueInstant");
    }

    public void CheckNameID(XmlDocument responseXml)
    {
        var nameIds = responseXml.GetElementsByTagName("NameID", "*");
        if (nameIds == null || nameIds.Count < 1)
            throw new SPIDValidationException("NameID not present");

        var nameId = nameIds[0];
        if (nameId == null || string.IsNullOrWhiteSpace(nameId.InnerText))
            throw new SPIDValidationException("NameID missing or empty");

        var nameIdFormat = nameId.Attributes["Format"];
        if (nameIdFormat == null || string.IsNullOrWhiteSpace(nameIdFormat.Value) || nameIdFormat.Value != Saml20Constants.NameIdentifierFormats.Transient)
            throw new SPIDValidationException($"NameID Format must be {Saml20Constants.NameIdentifierFormats.Transient}");

        var nameIdNameQualifier = nameId.Attributes["NameQualifier"];
        if (nameIdNameQualifier == null || string.IsNullOrWhiteSpace(nameIdNameQualifier.Value))
            throw new SPIDValidationException("invalid NameID NameQualifier");
    }

    public void CheckResponseIssuer(XmlDocument responseXml)
    {
        var issuers = responseXml.GetElementsByTagName("Issuer", "*");
        if (issuers == null || issuers.Count < 1)
            throw new SPIDValidationException("Response Issuer not present");

        var issuer = issuers[0];
        var issuerFormat = issuer.Attributes["Format"];
        if (issuerFormat != null && issuerFormat.Value != Saml20Constants.NameIdentifierFormats.Entity)
            throw new SPIDValidationException($"Issuer format must be {Saml20Constants.NameIdentifierFormats.Entity}");
    }

    public void CheckAuthnContextClassRef(XmlDocument responseXml)
    {
        var AuthnContextClassRef = responseXml.GetElementsByTagName("AuthnContextClassRef", "*");
        if (AuthnContextClassRef == null || AuthnContextClassRef.Count < 1)
            throw new SPIDValidationException("AuthnContextClassRef not present");

        var validSpidL = _spidOptions.ValidSPIDL;
        var spidLUri = _spidOptions.SPIDLUri;
        var validAuthnContextClassRefs = validSpidL.Select(l => string.Format(spidLUri, l));

        if (!validAuthnContextClassRefs.Any(v => v == AuthnContextClassRef[0].InnerText))
            throw new SPIDValidationException("invalid AuthnContextClassRef");
    }

    private bool IsValidUTC(string dateTime)
    {
        if (!dateTime.EndsWith("Z"))
            return false;

        DateTimeOffset dt;
        bool parsed = DateTimeOffset.TryParse(dateTime, out dt);

        if (!parsed)
            return false;

        if (dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0 && dt.Millisecond == 0)
            return false;

        return true;
    }

    private bool IsCIE(string issuer)
    {
        return cieIssuers.Contains(issuer);
    }
}