using System.Net;
using System.Net.Http;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.UI.API
{
    public class VersionCheckHttpHandler : DelegatingHandler
    {
        private readonly ILogger _log;

        public VersionCheckHttpHandler(ILogger<VersionCheckHttpHandler> log)
        {
            this._log = log;
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
            => this.SendAsync(request, cancellationToken).GetAwaiter().GetResult();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add(CustomHeaders.ClientVersion, EinherjiInfo.WebVersion);
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.BadRequest && response.Headers.Contains(CustomHeaders.ExpectedClientVersion))
            {
                this._log.LogError("Client version is outdated");
                throw new ClientVersionException();
            }
            return response;
        }
    }
}
