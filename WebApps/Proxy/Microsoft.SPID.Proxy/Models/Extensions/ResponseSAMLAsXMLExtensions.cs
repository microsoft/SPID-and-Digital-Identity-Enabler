/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models.Extensions;

public static class ResponseSAMLAsXMLExtensions
{
	public static XmlDocument AlterSubjectConfirmation(this XmlDocument samlResponse,
		string attributeConsumerServiceUrl)
	{
		var subjectConfirmationData = samlResponse.GetElementsByTagName("SubjectConfirmationData", "*");
		var recipient = subjectConfirmationData[0].Attributes["Recipient"];
		var newRecipient = attributeConsumerServiceUrl;
		recipient.Value = newRecipient;

		return samlResponse;
	}

	public static XmlDocument RemoveSignatures(this XmlDocument samlResponse)
	{
		var signatures = samlResponse.GetElementsByTagName("Signature", "*");
		if (signatures == null)
			return null;
		for (int i = signatures.Count - 1; i >= 0; i--)
		{
			signatures[i].ParentNode.RemoveChild(signatures[i]);
		}

		return samlResponse;
	}
	public static XmlDocument AlterAudience(this XmlDocument samlResponse,
		string originalEntityId)
	{
		var audience = samlResponse.GetElementsByTagName("Audience", "*");
		if (string.IsNullOrWhiteSpace(audience[0].InnerText))
			return null;
		var newAudience = originalEntityId; ;
		audience[0].InnerText = newAudience;

		return samlResponse;
	}

	public static XmlDocument AlterDestination(this XmlDocument samlResponse,
		string attributeConsumerServiceUrl,
		string baseHost,
		bool skipTechnicalChecks)
	{
		var root = samlResponse.DocumentElement;
		if (!root.HasAttribute("Destination") || string.IsNullOrWhiteSpace(root.GetAttribute("Destination")))
			throw new SPIDValidationException("missing Destination");
		var destValue = root.GetAttribute("Destination");
		if (destValue != $"https://{baseHost}/proxy/assertionconsumer" && !skipTechnicalChecks)
			throw new SPIDValidationException("different Destination");

		var newDestination = attributeConsumerServiceUrl;
		root.SetAttribute("Destination", newDestination);

		return samlResponse;
	}

	public static XmlDocument RemoveNameQualifierIfFormatEntity(this XmlDocument samlResponse)
	{
		var issuers = samlResponse.GetElementsByTagName("Issuer", Saml20Constants.ASSERTION);
		foreach (XmlNode issuer in issuers)
		{
			var format = issuer.Attributes["Format"];
			if (format == null)
				continue;
			var nameQualifier = issuer.Attributes["NameQualifier"];

			if (!string.IsNullOrWhiteSpace(format.Value) && format.Value.Equals(Saml20Constants.NameIdentifierFormats.Entity))
			{
				if (nameQualifier != null)
				{
					issuer.Attributes.Remove(nameQualifier);
				}
			}
		}

		return samlResponse;
	}

	public static string GetIssuer(this XmlDocument samlDocument)
	{
		return samlDocument.GetElementsByTagName("Issuer", "*")[0]?.InnerText;
	}

	public static string GetAttribute(this XmlDocument samlResponse, string key)
	{
		var value = string.Empty;

		var subject = samlResponse.GetElementsByTagName("Attribute", "*").Cast<XmlNode>();
		value = subject
			.Where(node => node.Attributes["Name"].Value == key)
			.SingleOrDefault()?
			.InnerText;

		return value;
	}

	public static string GetInResponseTo(this XmlDocument samlResponse)
	{
		return samlResponse.GetElementsByTagName("Response", "*")[0]?.Attributes["InResponseTo"]?.Value;
	}

	public static string GetResponseID(this XmlDocument samlDocument)
	{
		return samlDocument.GetElementsByTagName("Response", "*")[0]?.Attributes["ID"]?.Value;
	}

	public static string GetRequestID(this XmlDocument samlDocument)
	{
		return samlDocument.GetElementsByTagName("AuthnRequest", "*")[0]?.Attributes["ID"]?.Value;
	}


}