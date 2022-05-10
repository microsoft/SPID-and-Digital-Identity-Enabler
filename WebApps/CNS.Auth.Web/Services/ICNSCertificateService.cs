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
		/// Validates the provided X509Certificate2 checking if it has the required CNS certificate policies
		/// </summary>
		/// <param name="cert">The X509Certificate2 to check</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns>true if the X509Certificate2 has the required CNS certificate policies. false otherwise</returns>
		Task<bool> ValidateCNSCertificate(X509Certificate2 certificate, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get ClaimsPrincipal represented by the X509Certificate2
		/// </summary>
		/// <param name="cert">The X509Certificate2 from which to extract the ClaimsPrincipal</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>The ClaimsPrincipal represented by the X509Certificate2</returns>
		Task<ClaimsPrincipal> GetClaimsPrincipal(X509Certificate2 certificate, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get CNS trusted root CAs from Trusted list file url
		/// </summary>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns>The X509Certificate2Collection representing the trusted root CAs for CNS certificates</returns>
		Task<X509Certificate2Collection> GetTrustedCertificateCollection(CancellationToken cancellationToken = default);

		/// <summary>
		/// Configure certificate authentication options by CertificateAuthenticationOptions object
		/// </summary>
		/// <param name="options">Option used to configure certificate authentication</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns></returns>
		Task ConfigureCertificateAuthenticationOptions(CertificateAuthenticationOptions options, CancellationToken cancellationToken = default);
	}
}
