using MudBlazor;
using TehGM.EinherjiBot.PlaceholdersEngine;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.Policies;
using TehGM.EinherjiBot.UI.Components.PlaceholdersEngine;

namespace TehGM.EinherjiBot.UI.PlaceholdersEngine.Services
{
    public class PlaceholdersBuilderOpener : IPlaceholdersBuilder
    {
        private readonly IDialogService _dialogService;
        private readonly IBotAuthorizationService _auth;

        public PlaceholdersBuilderOpener(IDialogService dialogService, IBotAuthorizationService auth)
        {
            this._dialogService = dialogService;
            this._auth = auth;
        }

        public async Task<PlaceholderBuilderResult> OpenAsync(PlaceholderUsage context, bool allowAdminContext = true)
        {
            if (allowAdminContext)
            {
                BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(typeof(AuthorizeBotOrAdmin));
                if (authorization.Succeeded)
                    context |= PlaceholderUsage.Admin;
            }

            DialogParameters parameters = new DialogParameters
            {
                { nameof(PlaceholdersBuilderDialog.Context), context }
            };

            DialogOptions options = new DialogOptions()
            {
                CloseOnEscapeKey = true,
                DisableBackdropClick = false,
                //FullScreen = true,
                FullWidth = true,
                NoHeader = true,
                CloseButton = true,
                MaxWidth = MaxWidth.Medium,
                Position = DialogPosition.CenterRight
            };

            IDialogReference dialog = this._dialogService.Show<PlaceholdersBuilderDialog>(PlaceholdersBuilderDialog.DefaultTitle, parameters, options);
            DialogResult result = await dialog.Result;
            if (result.Cancelled)
                return PlaceholderBuilderResult.Cancelled;
            return (PlaceholderBuilderResult)result.Data;
        }
    }
}
