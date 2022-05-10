/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CNS.Auth.Web.Services
{
	/// <summary>
	/// Rapresente the SAML Service for response, request and metadata
	/// </summary>
	public interface ISAMLService
	{
		/// <summary>
		/// Create a SAML response by XML document and principal claims
		/// </summary>
		/// <param name="SAMLRequest">Xml document object</param>
		/// <param name="principal">Principal Claims</param>
		/// <param name="sign">bool value for sign status</param>
		/// <param name="cancellationToken">cancellation token</param>
		/// <returns></returns>
		Task<XDocument> CreateSAMLResponse(XDocument SAMLRequest, ClaimsPrincipal principal, bool sign = false, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sign SAML response 
		/// </summary>
		/// <param name="SAMLResponse">XDocument document object</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		Task<XDocument> SignSAMLResponse(XDocument SAMLResponse, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get decoded and inflated SAML request
		/// </summary>
		/// <param name="SAMLRequest">base 64 SAML request</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Parsed Xdocuent of SAML request</returns>
		Task<XDocument> GetDecodedInflatedSAMLRequest(string SAMLRequest, CancellationToken cancellationToken = default);

		/// <summary>
		/// Create XDocument metadata
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<XDocument> GetMetadata(CancellationToken cancellationToken = default);
	}
}
