/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.Extensions.Logging.EventLog;
this is an intentional wrong line;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHttpClient();

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

builder.Services.Configure<AttributeConsumingServiceOptions>(options => builder.Configuration.GetSection("attributeConsumingService").Bind(options));
builder.Services.Configure<AttributeConsumingServiceOptions>(options => options.ValidACS = builder.Configuration.GetValue<string>("attributeConsumingService:validACS").Split(",").ToList());
builder.Services.Configure<CertificateOptions>(options => builder.Configuration.GetSection("Certificate").Bind(options));
builder.Services.Configure<CustomErrorOptions>(options => builder.Configuration.GetSection("customErrors").Bind(options));
builder.Services.Configure<FederatorOptions>(options => builder.Configuration.GetSection("Federator").Bind(options));
builder.Services.Configure<IDPMetadatasOptions>(options => builder.Configuration.GetSection("idpMetadatas").Bind(options));
builder.Services.Configure<LogAccessOptions>(options => builder.Configuration.GetSection("LogAccess").Bind(options));
builder.Services.Configure<LogAccessOptions>(options => options.FieldsToLog = builder.Configuration.GetValue<string>("LogAccess:FieldsToLog").Split(",").ToList());
builder.Services.Configure<SPIDOptions>(options => builder.Configuration.GetSection("spid").Bind(options));
builder.Services.Configure<TechnicalChecksOptions>(options => builder.Configuration.GetSection("TechnicalChecks").Bind(options));
builder.Services.Configure<EventLogSettings>(options => builder.Configuration.GetSection("Logging:EventLog:Settings").Bind(options));

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
