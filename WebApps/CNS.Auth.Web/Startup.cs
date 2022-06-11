/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CNS.Auth.Web.Middlewares;
using CNS.Auth.Web.Services;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens.Saml2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CNS.Auth.Web
{
	public class Startup
	{
		public IConfiguration Configuration { get; }
		public IWebHostEnvironment Env { get; }

		public Startup(IConfiguration configuration, IWebHostEnvironment env)
		{
			Configuration = configuration;
			Env = env;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			intentional error
			services.AddApplicationInsightsTelemetry();
			services.AddControllersWithViews().
				AddRazorRuntimeCompilation();

			services.AddCertificateForwarding(options =>
			{
				options.CertificateHeader = Configuration["CertificateForwarding:CertificateHeaderName"];

				bool isCertUrlEncoded = false;
				_ = bool.TryParse(Configuration["CertificateForwarding:IsCertUrlEncoded"], out isCertUrlEncoded);
				if (isCertUrlEncoded)
				{
					options.HeaderConverter = (headerValue) =>
					{
						X509Certificate2 clientCertificate = null;

						if (!string.IsNullOrWhiteSpace(headerValue))
						{
							string certPem = WebUtility.UrlDecode(headerValue);
							clientCertificate = X509Certificate2.CreateFromPem(certPem, null);
						}

						return clientCertificate;
					};
				}

			});
			services.Configure<CNSCertificateServiceOptions>(Configuration.GetSection(CNSCertificateServiceOptions.CNSCertificate));
			services.AddSingleton<ICNSCertificateService, CNSCertificateService>();

			services.Configure<SAMLServiceOptions>(options =>
			{
				var certThumbprint = Configuration["SAMLService:SigningCertificateThumbprint"];
				using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
				{
					certStore.Open(OpenFlags.ReadOnly);

					X509Certificate2Collection certCollection = certStore.Certificates.Find(
												X509FindType.FindByThumbprint,
												certThumbprint,
												false);
					// Get the first cert with the thumbprint (should be only one)
					var signingCert = certCollection.OfType<X509Certificate2>().FirstOrDefault();

					if (signingCert is null)
						throw new Exception($"Certificate with thumbprint {certThumbprint} was not found");
					Configuration.Bind("SAMLService", options);
					options.SigningCertificate = signingCert;
				}
			});
			services.AddSingleton<Saml2SecurityTokenHandler>(sp =>
			{
				return new Saml2SecurityTokenHandler()
				{
					SetDefaultTimesOnTokenCreation = true,
					TokenLifetimeInMinutes = 60
				};
			});
			services.AddSingleton<ISAMLService, SAMLService>();
			services.AddHttpClient();
			var sp = services.BuildServiceProvider();
			var cnsService = sp.GetRequiredService<ICNSCertificateService>();

			var authBuilder = services
			.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
				.AddCertificate(async options =>
				{
					await cnsService.ConfigureCertificateAuthenticationOptions(options);
				});

			if (Env.IsProduction())
			{
				authBuilder.AddCertificateCache();
			}

			services.AddAuthorization();
			services.AddHostedService<TrustedListUpdaterService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error?code=500");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");

			app.UseRouting();

			app.UseCertificateForwarding();
			app.UseMiddleware<CNSCertificateLoggerMiddleware>();
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}

	}
}
