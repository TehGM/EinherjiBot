using Newtonsoft.Json;
using System.Net.Http;

namespace TehGM.EinherjiBot.UI.API
{
    public static class ApiHttpClientExtensions
    {
        public static async Task<TResponse> SendJsonAsync<TResponse>(this IApiClient client, HttpRequestMessage request, object data, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpResponseMessage response = await client.SendAsync(request, data, cancellationToken).ConfigureAwait(false);
                string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(json);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        // GET
        public static async Task<HttpResponseMessage> GetAsync(this IApiClient client, string url, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            return await client.SendAsync(request, null, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<TResponse> GetJsonAsync<TResponse>(this IApiClient client, string url, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            return await SendJsonAsync<TResponse>(client, request, null, cancellationToken).ConfigureAwait(false);
        }

        // POST
        public static async Task<HttpResponseMessage> PostAsync(this IApiClient client, string url, object data, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            return await client.SendAsync(request, data, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<TResponse> PostJsonAsync<TResponse>(this IApiClient client, string url, object data, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            return await SendJsonAsync<TResponse>(client, request, data, cancellationToken).ConfigureAwait(false);
        }

        // PUT
        public static async Task<HttpResponseMessage> PutAsync(this IApiClient client, string url, object data, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
            return await client.SendAsync(request, data, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<TResponse> PutJsonAsync<TResponse>(this IApiClient client, string url, object data, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
            return await SendJsonAsync<TResponse>(client, request, data, cancellationToken).ConfigureAwait(false);
        }

        // DELETE
        public static async Task<HttpResponseMessage> DeleteAsync(this IApiClient client, string url, object data, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
            return await client.SendAsync(request, data, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<TResponse> DeleteJsonAsync<TResponse>(this IApiClient client, string url, object data, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
            return await SendJsonAsync<TResponse>(client, request, data, cancellationToken).ConfigureAwait(false);
        }
    }
}
