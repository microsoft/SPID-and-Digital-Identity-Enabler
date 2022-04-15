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
	public interface ISAMLService
	{
		Task<XDocument> CreateSAMLResponse(XDocument SAMLRequest, ClaimsPrincipal principal, bool sign = false, CancellationToken cancellationToken = default);
		Task<XDocument> SignSAMLResponse(XDocument SAMLResponse, CancellationToken cancellationToken = default);
		Task<XDocument> GetDecodedInflatedSAMLRequest(string SAMLRequest, CancellationToken cancellationToken = default);
		Task<XDocument> GetMetadata(CancellationToken cancellationToken = default);
	}
}
