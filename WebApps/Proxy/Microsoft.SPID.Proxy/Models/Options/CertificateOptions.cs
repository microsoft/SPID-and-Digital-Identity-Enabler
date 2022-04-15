/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models.Options;

public class CertificateOptions
{
    public string CertName { get; set; }
    public string CertPassword { get; set; }
    public int CacheAbsoluteExpirationMinutes { get; set; } = 360;
}