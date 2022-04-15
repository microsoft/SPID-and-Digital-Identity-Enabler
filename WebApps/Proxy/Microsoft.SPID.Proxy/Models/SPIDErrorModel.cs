/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.AspNetCore.Html;

namespace Microsoft.SPID.Proxy.Models;

public class SPIDErrorModel
{
    public HtmlString StatusCode { get; set; }
    public HtmlString StatusMessage { get; set; }
    public HtmlString UserFriendlyMessage { get; set; }
}