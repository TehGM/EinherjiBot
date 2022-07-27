using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Web;
using TehGM.EinherjiBot.Security.API;

namespace TehGM.EinherjiBot.UI.Security.Services
{
    public class DiscordLoginRedirect : IDiscordLoginRedirect
    {
        private const string _storageKey = "oauth_state";

        private readonly NavigationManager _navigation;
        private readonly ILocalStorageService _localStorage;
        private readonly DiscordAuthOptions _options;

        public DiscordLoginRedirect(ILocalStorageService localStorage, NavigationManager navigation, IOptionsSnapshot<DiscordAuthOptions> options)
        {
            this._localStorage = localStorage;
            this._navigation = navigation;
            this._options = options.Value;
        }

        public async Task RedirectAsync(CancellationToken cancellationToken = default)
        {
            OAuthState state = new OAuthState(this._navigation.Uri);
            await this._localStorage.SetItemAsync(_storageKey, state, cancellationToken).ConfigureAwait(false);

            IDictionary<string, string> query = new Dictionary<string, string>(4)
            {
                { "client_id", this._options.ClientID },
                { "redirect_uri", this._options.RedirectURL },
                { "response_type", "code" },
                { "state", state.ToString() }
            };
            if (this._options.Scopes?.Any() == true)
                query.Add("scope", string.Join(' ', this._options.Scopes));

            string queryString = string.Join('&', query.Select(pair => $"{pair.Key}={HttpUtility.UrlEncodeUnicode(pair.Value)}"));
            string url = $"https://discord.com/api/oauth2/authorize?" + queryString;

            this._navigation.NavigateTo(url, false, false);
        }

        public async Task<bool> ValidateStateAsync(string state, CancellationToken cancellationToken = default)
        {
            if (!await this._localStorage.ContainKeyAsync(_storageKey, cancellationToken).ConfigureAwait(false))
                return false;

            OAuthState expected = await this._localStorage.GetItemAsync<OAuthState>(_storageKey, cancellationToken).ConfigureAwait(false);
            return expected.ToString() == state;
        }
    }
}
