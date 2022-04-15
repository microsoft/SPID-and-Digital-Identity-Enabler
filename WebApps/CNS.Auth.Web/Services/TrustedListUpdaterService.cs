/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace CNS.Auth.Web.Services
{
	public class TrustedListUpdaterService : IHostedService, IDisposable
	{
		private readonly IOptionsMonitorCache<CertificateAuthenticationOptions> _certOptionsMonitor;
		private readonly ICNSCertificateService _cnsCertService;
		private readonly CNSCertificateServiceOptions _cnsCertOptions;
		private readonly ILogger<TrustedListUpdaterService> _log;
		private Timer _timer;

		public TrustedListUpdaterService(IOptionsMonitorCache<CertificateAuthenticationOptions> certOptionsMonitor,
			IOptions<CNSCertificateServiceOptions> cnsCertOptions,
			ICNSCertificateService cnsCertService,
			ILogger<TrustedListUpdaterService> log)
		{
			_certOptionsMonitor = certOptionsMonitor ?? throw new ArgumentNullException(nameof(certOptionsMonitor));
			_cnsCertService = cnsCertService ?? throw new ArgumentNullException(nameof(cnsCertService));
			_cnsCertOptions = cnsCertOptions.Value;
			_log = log ?? throw new ArgumentNullException(nameof(log));
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_log.LogInformation("StartAsync invoked: starting timer to trigger Trusted Root CAs update. The timer will trigger every {hours} hours", _cnsCertOptions.TrustedListPeriodicUpdateHours);
#if DEBUG
			_timer = new Timer(UpdateTrustedList, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(10));
#else
			var timeSpan = TimeSpan.FromHours(_cnsCertOptions.TrustedListPeriodicUpdateHours);
			_timer = new Timer(UpdateTrustedList, null, timeSpan, timeSpan);
#endif
			_log.LogInformation("Timer configured");
			return Task.CompletedTask;
		}

		private async void UpdateTrustedList(object state)
		{
			_log.LogInformation("Removing the CertificateAuthenticationOptions from cache");
			//_certOptionsMonitor.TryRemove(CertificateAuthenticationDefaults.AuthenticationScheme);
			_certOptionsMonitor.Clear();
			_log.LogInformation("CertificateAuthenticationOptions removed from cache");

			var newOptions = new CertificateAuthenticationOptions();
			await _cnsCertService.GetCertificateAuthenticationOptions(newOptions);
			_log.LogInformation("Adding the new CertificateAuthenticationOptions to cache");
			_certOptionsMonitor.TryAdd(CertificateAuthenticationDefaults.AuthenticationScheme, newOptions);
			_log.LogInformation("New CertificateAuthenticationOptions added to cache");
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_log.LogInformation("StopAsync invoked: stopping timer.");
			_timer?.Change(Timeout.Infinite, Timeout.Infinite);
			_log.LogInformation("Timer stopped");
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_timer?.Dispose();
		}
	}
}
