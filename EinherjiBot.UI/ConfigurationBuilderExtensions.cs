using System.IO;
using System.Net.Http;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static async Task<IConfigurationBuilder> AddJsonFileAsync(this IConfigurationBuilder builder, HttpClient client, string filename, bool optional = true, CancellationToken cancellationToken = default)
        {
            using HttpResponseMessage response = await client.GetAsync(filename, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode && optional)
                return builder;
            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            builder.AddJsonStream(stream);
            return builder;
        }
    }
}
