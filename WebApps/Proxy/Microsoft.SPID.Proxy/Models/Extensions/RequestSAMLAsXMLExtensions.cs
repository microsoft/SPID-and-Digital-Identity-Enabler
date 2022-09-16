/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Xml;

namespace Microsoft.SPID.Proxy.Models.Extensions;

public static class RequestSAMLAsXMLExtensions
{
    public static XmlDocument ChangeIssuer(this XmlDocument samlRequest, string spidEntityId)
    {
		XmlElement rootEl = samlRequest.DocumentElement;
		XmlNodeList IssuerTags = rootEl.GetElementsByTagName("Issuer", "*");

		//Update required Issuer attributes only if present
		if (IssuerTags.Count > 0)
		{
			XmlNode IssuerTag = IssuerTags[0]; //you can have only 1 Issuer
			XmlAttribute FormatAttribute = samlRequest.CreateAttribute("Format");
			FormatAttribute.Value = "urn:oasis:names:tc:SAML:2.0:nameid-format:entity";
			IssuerTag.Attributes.Append(FormatAttribute);
			XmlAttribute nameQualifier = samlRequest.CreateAttribute("NameQualifier");
			nameQualifier.Value = spidEntityId;
			IssuerTag.Attributes.Append(nameQualifier);
			IssuerTag.InnerText = spidEntityId;
		}

		return samlRequest;
	}

	public static bool SAMLRequestIsLogout(this XmlDocument samlRequest)
	{
		return samlRequest.DocumentElement.Name == "samlp:LogoutRequest";
	}

	public static XmlDocument SetAttributeConsumingService(this XmlDocument samlRequest,
		int attributeConsumerService)
    {
		XmlElement rootEl = samlRequest.DocumentElement;

		var AtCS = rootEl.GetAttributeNode("AttributeConsumingServiceIndex", "*");
		if (AtCS != null)
		{
			AtCS.Value = attributeConsumerService.ToString();
		}
		else
		{
			XmlAttribute AttributeConsumingService = samlRequest.CreateAttribute("AttributeConsumingServiceIndex");
			AttributeConsumingService.Value = attributeConsumerService.ToString();
			rootEl.Attributes.Append(AttributeConsumingService);
		}

		return samlRequest;
	}

	public static XmlDocument AddRequestedAuthnContext(this XmlDocument samlRequest, 
		string requestedAuthnContextClassRef)
    {
		XmlElement rootEl = samlRequest.DocumentElement;
		XmlNode RequestedAuthnContext = samlRequest.CreateElement("samlp", "RequestedAuthnContext", "urn:oasis:names:tc:SAML:2.0:protocol");

		rootEl.AppendChild(RequestedAuthnContext);

		return samlRequest;
    }

	public static XmlDocument SetAuthnContextClassRefIfNotPresent(this XmlDocument samlRequest, 
		string requestedAuthnContextClassRef)
    {
		XmlElement rootEl = samlRequest.DocumentElement;
		var RequestedAuthnContext = samlRequest.GetElementsByTagName("RequestedAuthnContext", "*")[0];

		var authnContextClassRefs = rootEl.GetElementsByTagName("AuthnContextClassRef", "*");
		if (authnContextClassRefs != null && authnContextClassRefs.Count == 0)
		{
			XmlNode AuthnContextClassRef = samlRequest.CreateElement("samlp", "AuthnContextClassRef", "urn:oasis:names:tc:SAML:2.0:assertion");

			AuthnContextClassRef.InnerText = requestedAuthnContextClassRef;

			RequestedAuthnContext.AppendChild(AuthnContextClassRef);
		}

		return samlRequest;
    }

	public static XmlDocument SetForceAuthn(this XmlDocument samlRequest)
    {
		XmlElement rootEl = samlRequest.DocumentElement;

		XmlAttribute forceAuthNAttr = rootEl.GetAttributeNode("ForceAuthn", "*");
		if (forceAuthNAttr == null)
		{
			forceAuthNAttr = samlRequest.CreateAttribute("ForceAuthn");
			forceAuthNAttr.Value = "true";
			rootEl.Attributes.Append(forceAuthNAttr);
		}
		else
		{
			forceAuthNAttr.Value = "true";
		}

		return samlRequest;
	}

	public static XmlDocument SetComparison(this XmlDocument samlRequest)
    {
		var RequestedAuthnContext = samlRequest.GetElementsByTagName("RequestedAuthnContext", "*")[0];
		var comparison = RequestedAuthnContext.Attributes["Comparison"];
		if (comparison == null)
		{
			XmlAttribute NewComparison = samlRequest.CreateAttribute("Comparison");
			NewComparison.Value = "minimum";
			RequestedAuthnContext.Attributes.Append(NewComparison);
		}
		else
		{
			comparison.Value = "minimum";
		}

		return samlRequest;
	}

	public static XmlDocument RemoveUncompliantAuthnContextClassrefs(this XmlDocument samlRequest)
    {
		XmlElement rootEl = samlRequest.DocumentElement;

		XmlNodeList authnContextClassRefs = rootEl.GetElementsByTagName("AuthnContextClassRef", "*");
		if (authnContextClassRefs != null && authnContextClassRefs.Count != 0)
		{
			var authnContextClassRefsToBeRemoved = new List<XmlNode>();

			for (var i = 0; i < authnContextClassRefs.Count; i++)
			{
				if (authnContextClassRefs[i].InnerText != "https://www.spid.gov.it/SpidL1"
					&& authnContextClassRefs[i].InnerText != "https://www.spid.gov.it/SpidL2"
					&& authnContextClassRefs[i].InnerText != "https://www.spid.gov.it/SpidL3")
				{
					authnContextClassRefsToBeRemoved.Add(authnContextClassRefs[i]);
				}
			}
			foreach (var authnContextClassRefToBeRemoved in authnContextClassRefsToBeRemoved)
			{
				authnContextClassRefToBeRemoved.ParentNode.RemoveChild(authnContextClassRefToBeRemoved);
			}
		}

		return samlRequest;
    }

	public static XmlDocument AddExtensionsAndPurposeIfNotPresent(this XmlDocument samlRequest,
		XmlNamespaceManager nameSpaceMgr,
		string samlpProtocolNamespace,
		string purposeValue)
	{
		XmlNodeList extensionElements = samlRequest.GetElementsByTagName("Extensions", "*");
		if (extensionElements.Count == 0 || samlRequest.GetElementsByTagName("Purpose", "*").Count == 0) // Ideally to check subtree only
		{
			XmlElement rootEl = samlRequest.DocumentElement;
			// Add the Spid Extension namespace at the current level
			var samlExtensionsNamespace = "https://spid.gov.it/saml-extensions";
			nameSpaceMgr.AddNamespace("spid", samlExtensionsNamespace);

			XmlElement Extensions = extensionElements.Count == 0 ? samlRequest.CreateElement("samlp:Extensions", samlpProtocolNamespace) : (XmlElement)extensionElements.Item(0);
			XmlElement Purpose = samlRequest.CreateElement("spid:Purpose", samlExtensionsNamespace);

			XmlAttribute SpidAttribute = samlRequest.CreateAttribute("xmlns:spid");
			SpidAttribute.Value = "https://spid.gov.it/saml-extensions";
			Extensions.Attributes.Append(SpidAttribute);

			Purpose.InnerText = purposeValue;

			Extensions.AppendChild(Purpose);
			var issuer = samlRequest.GetElementsByTagName("Issuer", "*")[0];

			rootEl.InsertAfter(Extensions,issuer);
		}

		return samlRequest;
	}
}