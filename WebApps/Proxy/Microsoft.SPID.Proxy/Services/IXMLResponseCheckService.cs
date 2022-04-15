/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Services;

public interface IXMLResponseCheckService
{
    void CheckAttributes(XmlDocument responseXml);

    void CheckSubjectConfirmation(XmlDocument responseXml);

    void CheckResponseInResponseTo(XmlDocument responseXml);

    void CheckResponseIssueInstant(XmlDocument responseXml);

    void CheckResponseVersion(XmlDocument responseXml);

    void CheckAssertion(XmlDocument responseXml);

    void CheckConditions(XmlDocument responseXml);

    void CheckSubjectConfirmationData(XmlDocument responseXml);

    void CheckNameID(XmlDocument responseXml);

    void CheckResponseIssuer(XmlDocument responseXml);

    void CheckAuthnContextClassRef(XmlDocument responseXml);

}