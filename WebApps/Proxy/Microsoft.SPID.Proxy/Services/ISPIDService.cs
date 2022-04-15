/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Specialized;

namespace Microsoft.SPID.Proxy.Services;

public interface ISPIDService
{
    int GetACSValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString);
    int GetSPIDLValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString);
    string GetPurposeValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString);
    bool IsSpidLValid(NameValueCollection queryStringCollection, string origin = "");
    bool IsPurposeValid(NameValueCollection queryStringCollection, string origin = "");
    bool IsACSValid(NameValueCollection queryStringCollection, string origin = "");
}