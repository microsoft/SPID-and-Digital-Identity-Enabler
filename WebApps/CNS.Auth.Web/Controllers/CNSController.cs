/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using CNS.Auth.Web.Models;
using CNS.Auth.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CNS.Auth.Web.Controllers
{
	public class CNSController : Controller
	{
		private readonly ILogger<CNSController> _log;
		private readonly ISAMLService samlService;

		public CNSController(ILogger<CNSController> log, ISAMLService samlService)
		{
			this._log = log ?? throw new ArgumentNullException(nameof(log));
			this.samlService = samlService ?? throw new ArgumentNullException(nameof(samlService));
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> Index(string SAMLRequest, string RelayState)
		{
			_log.LogInformation("Received a SAMLRequest, producing corresponding SAMLResponse with CNS certificate claims");
			XDocument decodedInflatedRequest = await samlService.GetDecodedInflatedSAMLRequest(SAMLRequest);
			XDocument samlResponse = await samlService.CreateSAMLResponse(decodedInflatedRequest, User, sign: true);
			_log.LogInformation("Signed SAMLResponse ready, returning AutoPosting form");

			AutoPostFormModel autoPostModel = new()
			{
				SAMLResponse = new Microsoft.AspNetCore.Html.HtmlString(Convert.ToBase64String(Encoding.UTF8.GetBytes(samlResponse.ToString(SaveOptions.DisableFormatting)))),
				RelayState = RelayState,
				ActionUrl = decodedInflatedRequest.Root.Attribute(XName.Get("AssertionConsumerServiceURL"))?.Value
			};

			return View(autoPostModel);
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Metadata()
		{
			XDocument metadata = await samlService.GetMetadata();

			return new ContentResult()
			{
				StatusCode = 200,
				ContentType = "text/xml; charset=utf-8",
				Content = metadata.ToString(SaveOptions.DisableFormatting)
			};
		}
	}
}
