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
	/// Represent the SAML Service for response, request and metadata
	/// </summary>
	public interface ISAMLService
	{

		/// <summary>
		/// Creates an XDocument representing the SAMLResponse
		/// </summary>
		/// <param name="SAMLRequest">The XDocument representing the incoming SAMLRequest</param>
		/// <param name="principal">The ClaimsPrincipal to put in the SAMLResponse</param>
		/// <param name="sign">A boolean value indicating if the SAMLResponse must be signed. Defaults to false</param>
		/// <param name="cancellationToken">cancellation token</param>
		/// <returns>An XDocument representing the SAMLResponse</returns>
		Task<XDocument> CreateSAMLResponse(XDocument SAMLRequest, ClaimsPrincipal principal, bool sign = false, CancellationToken cancellationToken = default);

		/// <summary>
		/// Signs the SAMLResponse represented in the XDocument 
		/// </summary>
		/// <param name="SAMLResponse">XDocument containing a SAMLResponse to sign</param>
		/// <param name="cancellationToken">Cancellation token</param>
		///<returns>The XDocument representing the signed SAMLResponse</returns>
		Task<XDocument> SignSAMLResponse(XDocument SAMLResponse, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get decoded and inflated SAML request
		/// </summary>
		/// <param name="SAMLRequest">base64 encoded and deflated SAMLRequest</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>XDocument representing the decoded and inflated SAMLRequest</returns>
		Task<XDocument> GetDecodedInflatedSAMLRequest(string SAMLRequest, CancellationToken cancellationToken = default);

		/// <summary>
		/// Create XDocument metadata
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<XDocument> GetMetadata(CancellationToken cancellationToken = default);
	}
}
