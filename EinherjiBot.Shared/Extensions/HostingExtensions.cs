using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Hosting
{
    public static class HostingExtensions
    {
        public static IHostBuilder ConfigureSecretsFiles(this IHostBuilder builder)
            => builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsecrets.json", optional: true);
                config.AddJsonFile($"appsecrets.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
            });
    }
}
