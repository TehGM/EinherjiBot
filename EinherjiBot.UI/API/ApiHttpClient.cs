using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http;
using System.Net.Http.Headers;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.API;
using TehGM.EinherjiBot.UI.Security;

namespace TehGM.EinherjiBot.UI.API.Services
{
    public class ApiHttpClient : IApiClient
    {
        private readonly HttpClient _client;
        private readonly NavigationManager _navigation;
        private readonly IDialogService _dialogs;
        private readonly ISnackbar _notifications;
        private readonly IAuthService _authService;
        private readonly IWebAuthProvider _authProvider;
        private readonly IRefreshTokenProvider _refreshTokenProvider;

        public ApiHttpClient(HttpClient client, NavigationManager navigation, IDialogService dialogs, ISnackbar notifications,
            IAuthService authService, IWebAuthProvider authProvider, IRefreshTokenProvider refreshTokenProvider)
        {
            this._client = client;
            this._navigation = navigation;
            this._dialogs = dialogs;
            this._notifications = notifications;
            this._authService = authService;
            this._authProvider = authProvider;
            this._refreshTokenProvider = refreshTokenProvider;
            this._client.BaseAddress = new Uri(navigation.BaseUri + "api/", UriKind.Absolute);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, object data, CancellationToken cancellationToken = default)
        {
            await this.AttachTokenAsync(request, cancellationToken).ConfigureAwait(false);
            try
            {
                return await this._client.SendJsonAsync(request, data, "application/json", cancellationToken).ConfigureAwait(false);
            }
            catch (ClientVersionException)
            {
                await this._dialogs.PromptForReload(this._navigation).ConfigureAwait(false);
                throw;
            }
            catch (ApiException ex)
            {
                this._notifications.Add(ex.Message, Severity.Error, options =>
                {
                    options.RequireInteraction = true;
                    options.SnackbarVariant = Variant.Filled;
                });
                throw;
            }
            catch
            {
                this._notifications.Add("An error has occured.", Severity.Error, options =>
                {
                    options.RequireInteraction = true;
                    options.SnackbarVariant = Variant.Filled;
                });
                throw;
            }
        }

        private async Task AttachTokenAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await this.RefreshAsync(cancellationToken).ConfigureAwait(false);
            if (this._authProvider.User.IsLoggedIn())
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this._authProvider.Token);
        }

        private async ValueTask RefreshAsync(CancellationToken cancellationToken)
        {
            if (!this._authProvider.User.IsLoggedIn())
                return;
            if (DateTime.UtcNow < this._authProvider.Expiration.AddSeconds(-5))
                return;
            string token = await this._refreshTokenProvider.GetAsync(cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(token))
            {
                LoginResponse response = await this._authService.RefreshAsync(token, cancellationToken).ConfigureAwait(false);
                await this._authProvider.LoginAsync(response, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
