/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPID.Proxy.Services;

public interface ICertificateService
{
    Task<X509Certificate2> GetProxySignCertificate();
}