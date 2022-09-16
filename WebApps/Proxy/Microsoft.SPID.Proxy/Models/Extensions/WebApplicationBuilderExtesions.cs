using Azure.Identity;

namespace Microsoft.SPID.Proxy.Models.Extensions
{
    public static class WebApplicationBuilderExtesions
    {
        public static WebApplicationBuilder AddKeyVaultConfigurationProvider(this WebApplicationBuilder builder)
        {
            string keyVaultName = builder.Configuration["Certificate:KeyVaultName"];

			if (!string.IsNullOrWhiteSpace(keyVaultName))
            {
                builder.Configuration.AddAzureKeyVault(
                new Uri($"https://{keyVaultName}.vault.azure.net/"),
                new DefaultAzureCredential());
            }

            return builder;
        }
    }
}
