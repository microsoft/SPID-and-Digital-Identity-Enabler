/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CNS.Auth.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Linq;
using System.Threading.Tasks;

namespace CNS.Auth.Web.Middlewares
{
	public class CNSCertificateLoggerMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly CertificateForwardingOptions _forwardingOptions;
		private readonly ILogger<CNSCertificateLoggerMiddleware> _log;
		private readonly CNSCertificateServiceOptions _options;

		public CNSCertificateLoggerMiddleware(RequestDelegate next,
			IOptions<CNSCertificateServiceOptions> options,
			IOptions<CertificateForwardingOptions> forwardingOptions,
			ILogger<CNSCertificateLoggerMiddleware> log)
		{
			_next = next;
			_forwardingOptions = forwardingOptions.Value;
			_log = log ?? throw new System.ArgumentNullException(nameof(log));
			_options = options.Value;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if (_options.LogCertificate)
			{
				var certHeader = context.Request.Headers[_forwardingOptions.CertificateHeader];
				if (certHeader != StringValues.Empty)
				{
					string certFromHeader = certHeader.FirstOrDefault();
					if (!string.IsNullOrWhiteSpace(certFromHeader))
					{
						_log.LogInformation("Certificate from {header} header: {certificate}", _forwardingOptions.CertificateHeader, certFromHeader);
					}
				}
			}
			// Call the next delegate/middleware in the pipeline
			await _next(context);
		}
	}
}
