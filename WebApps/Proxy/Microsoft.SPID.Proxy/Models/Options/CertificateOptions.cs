/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models.Options;

public class CertificateOptions
{
    public string KeyVaultName { get; set; }
    public CertificateLocation CertLocation { get; set; }
    /// <summary>
    /// If the CertLocation is ServerRelativeStorage, define the filename in the server relative file folder SigningCert
    /// if the CertLocation is KeyVault, define the certificate name loaded from the KeyVault
    /// </summary>
    public string CertName { get; set; }
    public string CertPassword { get; set; }
    public int CacheAbsoluteExpirationMinutes { get; set; } = 360;
}

public enum CertificateLocation
{
    ServerRelativeStorage = 0,
    KeyVault = 1
}