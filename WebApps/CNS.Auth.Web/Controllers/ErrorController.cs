/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CNS.Auth.Web.Controllers
{
	public class ErrorController : Controller
	{
		private readonly ILogger<ErrorController> log;

		public ErrorController(ILogger<ErrorController> log)
		{
			this.log = log ?? throw new ArgumentNullException(nameof(log));
		}
		public IActionResult Index(string code)
		{
			log.LogInformation($"Rendering page {code}.cshtml");
			return View($"~/Views/Shared/{code}.cshtml");
		}
	}
}
