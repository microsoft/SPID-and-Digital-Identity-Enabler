/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging.EventLog;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.AddKeyVaultConfigurationProvider();

if (string.Equals(builder.Configuration["ASPNETCORE_FORWARDEDHEADERS_ENABLED"], "true", StringComparison.OrdinalIgnoreCase))
{
	builder.Services.AddTransient<IConfigureOptions<ForwardedHeadersOptions>, ConfigureForwardedHeadersOptions>();
}

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHttpClient("default", client =>
{
	bool userAgentEnabled = false;
	bool.TryParse(builder.Configuration["UserAgent:Enabled"], out userAgentEnabled);
	if(!userAgentEnabled)
	{
		return;
	}
	var userAgent = builder.Configuration["UserAgent:Value"];
	client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
});

builder.Services.AddControllersWithViews()
	.AddNewtonsoftJson(options =>
	{
		options.UseMemberCasing();
	});

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<ICertificateService, CertificateService>();
builder.Services.AddSingleton<IFederatorRequestService, FederatorRequestService>();
builder.Services.AddSingleton<IFederatorResponseService, FederatorResponseService>();
builder.Services.AddSingleton<ILogAccessService, LogAccessService>();
builder.Services.AddSingleton<ISAMLService, SAMLService>();
builder.Services.AddSingleton<ISPIDService, SPIDService>();
builder.Services.AddSingleton<IIDPService, IDPService>();
builder.Services.AddSingleton<IXMLResponseCheckService, XMLResponseCheckService>();

builder.Services.Configure<AttributeConsumingServiceOptions>(options => {
	var acsSection = builder.Configuration.GetSection("attributeConsumingService");
	acsSection.Bind(options);
	options.ValidACS = acsSection["validACS"].Split(",").ToList();
});

builder.Services.Configure<CertificateOptions>(builder.Configuration.GetSection("Certificate"));
builder.Services.Configure<CustomErrorOptions>(builder.Configuration.GetSection("customErrors"));
builder.Services.Configure<FederatorOptions>(builder.Configuration.GetSection("Federator"));
builder.Services.Configure<IDPMetadatasOptions>(builder.Configuration.GetSection("idpMetadatas"));
builder.Services.Configure<LogAccessOptions>(options => {
	var spidProxyLoggingSection = builder.Configuration.GetSection("SPIDProxyLogging:LogAccess");
	spidProxyLoggingSection.Bind(options);
	options.FieldsToLog = spidProxyLoggingSection["FieldsToLog"].Split(",").ToList();
});

builder.Services.Configure<SPIDOptions>(builder.Configuration.GetSection("spid"));
builder.Services.Configure<CIEOptions>(builder.Configuration.GetSection("cie"));
builder.Services.Configure<TechnicalChecksOptions>(builder.Configuration.GetSection("TechnicalChecks"));
builder.Services.Configure<EventLogSettings>(builder.Configuration.GetSection("Logging:EventLog:Settings"));
builder.Services.Configure<LoggingOptions>(builder.Configuration.GetSection("SPIDProxyLogging"));
builder.Services.Configure<OptionalResponseAlterationOptions>(builder.Configuration.GetSection("OptionalResponseAlteration"));

builder.Services
	.AddMvc()
	.AddRazorRuntimeCompilation();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}
else
{
	app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
