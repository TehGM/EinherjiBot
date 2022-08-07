using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.API;
using TehGM.EinherjiBot.UI.API;

namespace TehGM.EinherjiBot.UI.Security.Services
{
    public class WebAuthService : IAuthService
    {
        private readonly HttpClient _client;
        private readonly NavigationManager _navigation;
        private readonly IDialogService _dialogs;
        private readonly ISnackbar _notifications;

        public WebAuthService(HttpClient client, NavigationManager navigation, IDialogService dialogs, ISnackbar notifications)
        {
            this._client = client;
            this._navigation = navigation;
            this._dialogs = dialogs;
            this._notifications = notifications;
            this._client.BaseAddress = new Uri(navigation.BaseUri + "api/", UriKind.Absolute);
        }

        public async Task<LoginResponse> LoginAsync(string accessCode, CancellationToken cancellationToken = default)
        {
            try
            {
                LoginRequest request = new LoginRequest(accessCode);
                return await this._client.PostJsonAsync<LoginResponse>("auth/token", request, cancellationToken);
            }
            catch (ClientVersionException)
            {
                await this._dialogs.PromptForReload(this._navigation).ConfigureAwait(false);
                throw;
            }
            catch (ApiException ex)
            {
                this.ShowErrorNotification(ex);
                throw;
            }
            catch
            {
                this.ShowErrorNotification();
                throw;
            }
        }

        public async Task<LoginResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                RefreshRequest request = new RefreshRequest(refreshToken);
                return await this._client.PostJsonAsync<LoginResponse>("auth/token/refresh", request, cancellationToken);
            }
            catch (ClientVersionException)
            {
                await this._dialogs.PromptForReload(this._navigation).ConfigureAwait(false);
                throw;
            }
            catch (HttpRequestException ex)
            {
                this.ShowErrorNotification(ex);
                throw;
            }
            catch
            {
                this.ShowErrorNotification();
                throw;
            }
        }

        public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                RefreshRequest request = new RefreshRequest(refreshToken);
                await this._client.DeleteJsonAsync("auth/token", request, cancellationToken);
            }
            catch (ClientVersionException)
            {
                await this._dialogs.PromptForReload(this._navigation).ConfigureAwait(false);
                throw;
            }
            catch (HttpRequestException ex)
            {
                this.ShowErrorNotification(ex);
                throw;
            }
            catch
            {
                this.ShowErrorNotification();
                throw;
            }
        }

        private void ShowErrorNotification(HttpRequestException exception)
        {
            this._notifications.Add(exception.Message, Severity.Error, options =>
            {
                options.RequireInteraction = true;
                options.SnackbarVariant = Variant.Text;
            });
        }

        private void ShowErrorNotification()
        {
            this._notifications.Add("An error has occured.", Severity.Error, options =>
            {
                options.RequireInteraction = true;
                options.SnackbarVariant = Variant.Text;
            });
        }
    }
}
