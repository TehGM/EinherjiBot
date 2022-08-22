using System.Net;
using System.Net.Http;
using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.PlaceholdersEngine;

namespace TehGM.EinherjiBot.UI.API.Handlers
{
    public class RequestExceptionsHttpHandler : DelegatingHandler
    {
        private readonly ILogger _log;

        public RequestExceptionsHttpHandler(ILogger<RequestExceptionsHttpHandler> log)
        {
            this._log = log;
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
            => this.SendAsync(request, cancellationToken).GetAwaiter().GetResult();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                this._log.LogError("API request failed with code {StatusCode} ({Status})", (int)response.StatusCode, response.ReasonPhrase);
                string message = null;
                if (response.Content != null)
                    message = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(message))
                    message = $"API request failed - {response.ReasonPhrase}";
                response.Content?.Dispose();

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    throw new AccessForbiddenException(message);
                else if (response.Headers.TryGetValues(CustomHeaders.ExceptionType, out IEnumerable<string> values))
                {
                    if (values.Contains(nameof(PlaceholderConvertException)))
                        throw new PlaceholderConvertException(message);
                    if (values.Contains(nameof(PlaceholderContextException)))
                        throw new PlaceholderContextException(message);
                    if (values.Contains(nameof(PlaceholderFormatException)))
                        throw new PlaceholderFormatException(message);
                }
                else
                    throw new HttpRequestException(message, null, response.StatusCode);
            }
            return response;
        }
    }
}
