using System.Net.Http;

namespace TehGM.EinherjiBot.UI.API.Handlers
{
    public class UserAgentHttpHandler : DelegatingHandler
    {
        public UserAgentHttpHandler() { }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("User-Agent", $"{EinherjiInfo.Name} Web Client v{EinherjiInfo.WebVersion}");
            return base.SendAsync(request, cancellationToken);
        }
    }
}
