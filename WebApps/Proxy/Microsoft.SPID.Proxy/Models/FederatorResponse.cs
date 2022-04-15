/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.AspNetCore.Html;

namespace Microsoft.SPID.Proxy.Models;

public class FederatorResponse
{
    public HtmlString SAMLResponse { get; set; }
    public HtmlString RelayState { get; set; }
    public HtmlString Action { get; set; }

    public FederatorResponse(string samlResponse, string relayState, string action)
    {
        SAMLResponse = new(samlResponse);
        RelayState = new(relayState);
        Action = new(action);
    }
}