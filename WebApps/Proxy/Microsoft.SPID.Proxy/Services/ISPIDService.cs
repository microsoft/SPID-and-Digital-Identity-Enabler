/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Specialized;

namespace Microsoft.SPID.Proxy.Services;

public interface ISPIDService
{
	int GetSPIDAttributeConsumigServiceValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString);
	int GetCIEAttributeConsumigServiceValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString);

	int GetSPIDLValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString, bool isCie);
	string GetComparisonValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString, bool isCie);
	string GetPurposeValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString);
    bool IsSpidLValid(NameValueCollection queryStringCollection, string origin = "");
    bool IsPurposeValid(NameValueCollection queryStringCollection, string origin = "");
	bool IsSPIDACSValid(NameValueCollection queryStringCollection, string origin = "");
	bool IsCIEACSValid(NameValueCollection queryStringCollection, string origin = "");

	bool IsComparisonValid(NameValueCollection queryStringCollection, string origin = "");
}