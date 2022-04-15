/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Services;

public interface IFederatorRequestService
{
    Task<string> GetRedirectUriAsync(FederatorRequest federatorRequest, XmlDocument requestAsXml);

    string GetPassThrowRedirectUri(FederatorRequest federatorRequest);
}