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
	/// <summary>
	/// Rapresent the CNS certificare service
	/// </summary>
	public interface ICNSCertificateService
	{

		/// <summary>
		/// Validate certificate with thumbprint for CNS Certificate policies
		/// </summary>
		/// <param name="cert">X509 Certificate</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns>Bool value for validation</returns>
		Task<bool> ValidateCNSCertificate(X509Certificate2 certificate, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get principla Claims from certificate
		/// </summary>
		/// <param name="cert">X509 Certificate</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Claims Principal for the specified identity </returns>
		Task<ClaimsPrincipal> GetClaimsPrincipal(X509Certificate2 certificate, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get trusted root CS from Trusted list file url
		/// </summary>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns>It Return a X509 Certificate collection</returns>
		Task<X509Certificate2Collection> GetTrustedCertificateCollection(CancellationToken cancellationToken = default);

		/// <summary>
		/// Get certificate authentication options by CertificateAuthenticationOptions object
		/// </summary>
		/// <param name="options">Option used to configure certificate authentication</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns></returns>
		Task GetCertificateAuthenticationOptions(CertificateAuthenticationOptions options, CancellationToken cancellationToken = default);
	}
}
