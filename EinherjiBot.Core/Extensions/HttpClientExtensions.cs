using Newtonsoft.Json;
using System.Text;

namespace System.Net.Http
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> SendJsonAsync(this HttpClient client, HttpRequestMessage request, object data, string contentType, CancellationToken cancellationToken = default)
        {
            if (data != null)
            {
                string json = JsonConvert.SerializeObject(data, Formatting.None);
                request.Content = new StringContent(json, Encoding.UTF8, contentType);
            }
            return client.SendAsync(request, cancellationToken);
        }

        public static async Task<TResponse> SendJsonAsync<TResponse>(this HttpClient client, HttpRequestMessage request, object data, string contentType, CancellationToken cancellationToken = default)
        {
            using HttpResponseMessage response = await SendJsonAsync(client, request, data, contentType, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TResponse>(json);
        }

        // POST
        public static async Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string url, object data, string contentType, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            return await SendJsonAsync<HttpResponseMessage>(client, request, data, contentType, cancellationToken).ConfigureAwait(false);
        }

        public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string url, object data, CancellationToken cancellationToken = default)
            => PostJsonAsync(client, url, data, "application/json", cancellationToken);

        public static async Task<TResponse> PostJsonAsync<TResponse>(this HttpClient client, string url, object data, string contentType, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            return await SendJsonAsync<TResponse>(client, request, data, contentType, cancellationToken).ConfigureAwait(false);
        }

        public static Task<TResponse> PostJsonAsync<TResponse>(this HttpClient client, string url, object data, CancellationToken cancellationToken = default)
            => PostJsonAsync<TResponse>(client, url, data, "application/json", cancellationToken);

        // DELETE
        public static async Task<HttpResponseMessage> DeleteJsonAsync(this HttpClient client, string url, object data, string contentType, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
            return await SendJsonAsync<HttpResponseMessage>(client, request, data, contentType, cancellationToken).ConfigureAwait(false);
        }

        public static Task<HttpResponseMessage> DeleteJsonAsync(this HttpClient client, string url, object data, CancellationToken cancellationToken = default)
            => DeleteJsonAsync(client, url, data, "application/json", cancellationToken);

        public static async Task<TResponse> DeleteJsonAsync<TResponse>(this HttpClient client, string url, object data, string contentType, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
            return await SendJsonAsync<TResponse>(client, request, data, contentType, cancellationToken).ConfigureAwait(false);
        }

        public static Task<TResponse> DeleteJsonAsync<TResponse>(this HttpClient client, string url, object data, CancellationToken cancellationToken = default)
            => DeleteJsonAsync<TResponse>(client, url, data, "application/json", cancellationToken);
    }
}
