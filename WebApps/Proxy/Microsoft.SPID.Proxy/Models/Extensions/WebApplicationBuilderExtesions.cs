using Azure.Identity;

namespace Microsoft.SPID.Proxy.Models.Extensions
{
    public static class WebApplicationBuilderExtesions
    {
        public static WebApplicationBuilder AddKeyVaultConfigurationProvider(this WebApplicationBuilder builder)
        {
            if (!string.IsNullOrWhiteSpace(builder.Configuration["Certificate:KeyVaultName"]))
            {
                builder.Configuration.AddAzureKeyVault(
                new Uri($"https://{builder.Configuration["Certificate:KeyVaultName"]}.vault.azure.net/"),
                new DefaultAzureCredential());
            }

            return builder;
        }
    }
}
