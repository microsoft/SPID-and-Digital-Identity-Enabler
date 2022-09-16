using Azure.Identity;

namespace Microsoft.SPID.Proxy.Models.Extensions
{
    public static class WebApplicationBuilderExtesions
    {
        public static WebApplicationBuilder AddKeyVaultConfigurationProvider(this WebApplicationBuilder builder)
        {
            if (!string.IsNullOrEmpty(builder.Configuration["KeyVaultName"]))
            {
                builder.Configuration.AddAzureKeyVault(
                new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
                new DefaultAzureCredential());
            }

            return builder;
        }
    }
}
