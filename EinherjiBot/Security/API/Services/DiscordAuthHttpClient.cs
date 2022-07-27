using Newtonsoft.Json;
using System.Net.Http;
using TehGM.EinherjiBot.Security.Services;

namespace TehGM.EinherjiBot.Security.API.Services
{
    public class DiscordAuthHttpClient : DiscordHttpClient, IDiscordAuthHttpClient, IDiscordHttpClient
    {
        private readonly DiscordAuthOptions _options;

        public DiscordAuthHttpClient(HttpClient client, IOptionsSnapshot<DiscordAuthOptions> options)
            : base(client)
        {
            this._options = options.Value;
        }

        public async Task<DiscordAccessTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/oauth2/token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", this._options.ClientID },
                { "client_secret", this._options.ClientSecret },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", this._options.RedirectURL }
            });

            using HttpResponseMessage response = await this.Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<DiscordAccessTokenResponse>(content);
        }

        public async Task<DiscordAccessTokenResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/oauth2/token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", this._options.ClientID },
                { "client_secret", this._options.ClientSecret },
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            });

            using HttpResponseMessage response = await this.Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<DiscordAccessTokenResponse>(content);
        }

        public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/oauth2/token/revoke");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", this._options.ClientID },
                { "client_secret", this._options.ClientSecret },
                { "token", refreshToken },
                { "token_type_hint", "refresh_token" }
            });

            using HttpResponseMessage response = await this.Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
    }
}
