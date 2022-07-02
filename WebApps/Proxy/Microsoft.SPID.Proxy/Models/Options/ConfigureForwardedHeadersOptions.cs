using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

namespace Microsoft.SPID.Proxy.Models.Options
{
	public class ConfigureForwardedHeadersOptions : IConfigureOptions<ForwardedHeadersOptions>
	{
		private readonly IConfiguration _configuration;
		public ConfigureForwardedHeadersOptions(IConfiguration config)
		{
			_configuration = config;
		}

		public void Configure(ForwardedHeadersOptions options)
		{
			var section = _configuration.GetSection("ForwardedHeaders");
			section.Bind(options);

			var knownProxies = section.GetValue<string>("KnownProxies")?.Split(',');
			var knownNetworks = section.GetValue<string>("KnownNetworks")?.Split(',');

			if (knownProxies?.Length > 0)
			{
				options.KnownProxies.Clear();
				foreach (var ip in knownProxies)
				{
					options.KnownProxies.Add(IPAddress.Parse(ip));
				}
			}

			if (knownNetworks?.Length > 0)
			{
				options.KnownNetworks.Clear();
				foreach (var network in knownNetworks)
				{
					var networkSplit = network.Split('/');
					var prefix = networkSplit[0];
					var prefixLength = int.Parse(networkSplit[1]);

					options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse(prefix), prefixLength));
				}
			}
		}
	}
}
