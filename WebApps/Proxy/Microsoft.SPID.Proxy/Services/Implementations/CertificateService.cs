/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.Extensions.Caching.Distributed;
using System.Runtime.Caching;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPID.Proxy.Services.Implementations;

public class CertificateService : ICertificateService
{
	private X509Certificate2 _certificate;
	private readonly IHostEnvironment _hostEnvironment;
	private readonly IDistributedCache _cache;
	private readonly ILogger _logger;
	private readonly FederatorOptions _federatorOptions;
	private readonly CertificateOptions _certificateOptions;

	public CertificateService(IHostEnvironment hostEnvironment,
		IDistributedCache cache,
		ILogger<CertificateService> logger,
		IOptions<FederatorOptions> federatorOptions,
		IOptions<CertificateOptions> certificateOptions)
	{
		_hostEnvironment = hostEnvironment;
		_cache = cache;
		_logger = logger;
		_federatorOptions = federatorOptions.Value;
		_certificateOptions = certificateOptions.Value;
	}

	public async Task<X509Certificate2> GetProxySignCertificate()
	{
		string certName = _certificateOptions.CertName;

		_logger.LogDebug("Getting {certName} certificate", certName);

		if (_certificate != null)
		{
			_logger.LogDebug("Returning pre-loaded certificate.");
			return _certificate;
		}

		if (_cache != null)
		{
			var certBytes = await _cache.GetAsync($"SigningCert_{certName}");
			if (certBytes != null)
			{
				_logger.LogDebug("Certificate retrieved from cache");
				_certificate = new X509Certificate2(certBytes);
				return _certificate;
			}
		}

		var certpath = Path.Combine(_hostEnvironment.ContentRootPath, "SigningCert", certName);

		_logger.LogDebug("Loading certificate from filesystem: {certPath}", certpath);

		_certificate =
			new X509Certificate2(certpath,
			_certificateOptions.CertPassword,
			X509KeyStorageFlags.MachineKeySet |
			X509KeyStorageFlags.PersistKeySet |
			X509KeyStorageFlags.Exportable);

		if (_cache != null)
		{
			_logger.LogDebug("Storing certificate in cache");
			var certBytes = _certificate.Export(X509ContentType.Pfx);
			var options = new DistributedCacheEntryOptions()
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_certificateOptions.CacheAbsoluteExpirationMinutes),
				SlidingExpiration = null
			};
			await _cache.SetAsync($"SigningCert_{certName}", certBytes, options);
		}

		return _certificate;
	}
}
