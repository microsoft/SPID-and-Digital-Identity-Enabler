/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.AspNetCore.Authentication.Certificate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace CNS.Auth.Web.Services
{
    public interface ICNSCertificateService
    {
        Task<bool> ValidateCNSCertificate(X509Certificate2 certificate, CancellationToken cancellationToken = default);
        Task<ClaimsPrincipal> GetClaimsPrincipal(X509Certificate2 certificate, CancellationToken cancellationToken = default);
        Task<X509Certificate2Collection> GetTrustedCertificateCollection(CancellationToken cancellationToken = default);
		Task GetCertificateAuthenticationOptions(CertificateAuthenticationOptions options, CancellationToken cancellationToken = default);
	}
}
