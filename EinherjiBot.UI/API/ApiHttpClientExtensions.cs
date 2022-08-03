using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace TehGM.EinherjiBot.UI.API
{
    public static class ApiHttpClientExtensions
    {
        // GET
        public static async Task<HttpResponseMessage> GetJsonAsync(this IApiClient client, string url, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            return await client.SendJsonAsync<HttpResponseMessage>(request, null, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<TResponse> GetJsonAsync<TResponse>(this IApiClient client, string url, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            return await client.SendJsonAsync<TResponse>(request, null, cancellationToken).ConfigureAwait(false);
        }

        // POST
        public static async Task<HttpResponseMessage> PostJsonAsync(this IApiClient client, string url, object data, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            return await client.SendJsonAsync<HttpResponseMessage>(request, data, cancellationToken).ConfigureAwait(false);
        }

        public static Task<TResponse> PostJsonAsync<TResponse>(this IApiClient client, string url, object data, CancellationToken cancellationToken = default)
            => PostJsonAsync<TResponse>(client, url, data, cancellationToken);

        // PUT
        public static async Task<HttpResponseMessage> PutJsonAsync(this IApiClient client, string url, object data, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
            return await client.SendJsonAsync<HttpResponseMessage>(request, data, cancellationToken).ConfigureAwait(false);
        }

        public static Task<TResponse> PutJsonAsync<TResponse>(this IApiClient client, string url, object data, CancellationToken cancellationToken = default)
            => PutJsonAsync<TResponse>(client, url, data, cancellationToken);

        // DELETE
        public static async Task<HttpResponseMessage> DeleteJsonAsync(this IApiClient client, string url, object data, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
            return await client.SendJsonAsync<HttpResponseMessage>(request, data, cancellationToken).ConfigureAwait(false);
        }

        public static Task<TResponse> DeleteJsonAsync<TResponse>(this IApiClient client, string url, object data, CancellationToken cancellationToken = default)
            => DeleteJsonAsync<TResponse>(client, url, data, cancellationToken);
    }
}
