/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models;

public class FederatorRequest
{
    public string SAMLRequest { get; set; }
    public string Signature { get; set; }
    public string RelayState { get; set; }
    public string SigAlg { get; set; }
    public string IdentityProvider { get; set; }

    public FederatorRequest(string identityProvider, string samlRequest, string relayState, string sigAlg, string signature)
    {
        SAMLRequest = samlRequest;
        IdentityProvider = identityProvider.ToUpper();
        RelayState = relayState.UpperCaseUrlEncode();
        SigAlg = sigAlg.UpperCaseUrlEncode();
        Signature = signature;
    }
}