using System.Net.Http;
using System.Net.Http.Headers;
using TehGM.EinherjiBot.UI.Security;

namespace TehGM.EinherjiBot.UI.API.Services
{
    public class ApiJwtHttpHandler : DelegatingHandler
    {
        private readonly IWebAuthProvider _provider;

        public ApiJwtHttpHandler(IWebAuthProvider provider)
        {
            this._provider = provider;
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.AttachToken(request);
            return base.Send(request, cancellationToken);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.AttachToken(request);
            return base.SendAsync(request, cancellationToken);
        }

        private void AttachToken(HttpRequestMessage request)
        {
            if (this._provider.IsLoggedIn)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this._provider.Token);
        }
    }
}
