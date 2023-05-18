/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Security.Cryptography.Xml;

namespace Microsoft.SPID.Proxy.Services;

public interface IFederatorResponseService
{
    FederatorResponse GetFederatorResponse(XmlDocument samlResponse, string relayState);

    void RunTechnicalChecks(XmlDocument responseXml);

    Task<bool> CheckSignature(XmlDocument responseXml);

    Task SignWholeResponseMessageAsync(XmlDocument doc, string responseDigestMethod = SignedXml.XmlDsigSHA256Url);

    bool ResponseHasBlockingStatusCode(XmlDocument responseXml, out SPIDErrorModel errorModel);

    string GetUserFriendlyMessage(string statusMessageText);

    Task SignAssertionAsync(XmlDocument doc, string assertionDigestMethod = SignedXml.XmlDsigSHA256Url);

    void ApplyOptionalResponseAlteration(XmlDocument doc);
}