using System.Net.Http;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.Security.Services
{
    public class DiscordHttpClient
    {
        public HttpClient Client { get; }

        public DiscordHttpClient(HttpClient client)
        {
            this.Client = client;
            this.Client.DefaultRequestHeaders.Add("User-Agent", $"EinherjiBot ({EinherjiInfo.RepositoryURL}, {EinherjiInfo.WebVersion})");
        }

        public Task<UserInfoResponse> GetCurrentUserAsync(string bearerToken, CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
            request.Headers.Add("Authorization", $"Bearer {bearerToken}");
            return this.Client.SendJsonAsync<UserInfoResponse>(request, null, null, cancellationToken);
        }
    }
}
