using Microsoft.AspNetCore.Components;
using System.Net.Http;

namespace TehGM.EinherjiBot.UI.API.Services
{
    public class ApiHttpClient : IApiClient
    {
        public HttpClient Client { get; }

        public ApiHttpClient(HttpClient client, NavigationManager navigation)
        {
            this.Client = client;
            this.Client.BaseAddress = new Uri(navigation.BaseUri + "api/", UriKind.Absolute);
            this.Client.DefaultRequestHeaders.Add("User-Agent", $"Einherji Web Client v{EinherjiInfo.BotVersion}");
        }
    }
}
