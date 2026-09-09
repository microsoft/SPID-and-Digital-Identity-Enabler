/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Specialized;

namespace Microsoft.SPID.Proxy.Services;

public interface ISAMLService
{
    string GetAttributeConsumerService();

    bool HasReferrer();

    NameValueCollection GetRefererQueryString();

    NameValueCollection GetRelayStateQueryString(NameValueCollection refererQueryString);

    NameValueCollection GetWCTXQueryString(NameValueCollection refererQueryString);

    Task<string> GetSignature(FederatorRequest federatorRequest, string samlRequest, string sigAlg, string relayState);

    Task<bool> ValidateFederatorRequestSignature(FederatorRequest federatorRequest);
}
