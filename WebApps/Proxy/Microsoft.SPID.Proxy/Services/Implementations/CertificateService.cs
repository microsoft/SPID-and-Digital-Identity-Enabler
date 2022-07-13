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
    private readonly IConfiguration _configuration;
    private readonly CertificateOptions _certificateOptions;

    public CertificateService(IHostEnvironment hostEnvironment,
        IDistributedCache cache,
        ILogger<CertificateService> logger,
        IConfiguration configuration,
        IOptions<CertificateOptions> certificateOptions)
    {
        _hostEnvironment = hostEnvironment;
        _cache = cache;
        _logger = logger;
        _configuration = configuration;
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

        _certificate = LoadCertificate();

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

    private X509Certificate2 LoadCertificate()
    {
        X509Certificate2 cert = null;
        switch (_certificateOptions.CertLocation)
        {
            case CertificateLocation.ServerRelativeStorage:
                var certpath = Path.Combine(_hostEnvironment.ContentRootPath, "SigningCert", _certificateOptions.CertName);
                _logger.LogDebug("Loading certificate from filesystem: {certPath}", certpath);
                cert =
                    new X509Certificate2(certpath,
                    _certificateOptions.CertPassword,
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet |
                    X509KeyStorageFlags.Exportable);
                break;
            case CertificateLocation.KeyVault:
                var keyVaultName = _configuration.GetValue<string>("KeyVaultName");
                if (!string.IsNullOrEmpty(keyVaultName))
                {
                    _logger.LogDebug($"Loading certificate from KeyVault: {keyVaultName}");
                    var certBase64 = _configuration.GetValue<string>(_certificateOptions.CertName);
                    cert = new X509Certificate2(Convert.FromBase64String(certBase64));
                }
                else
                {
                    throw new InvalidOperationException($"The CertificateLocation is KeyVault, but KeyVaultName is null");
                }
                break;
        }
        return cert;
    }
}
