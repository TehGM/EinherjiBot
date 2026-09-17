using MudBlazor;
using Microsoft.AspNetCore.Components;

namespace TehGM.EinherjiBot.UI
{
    public static class DialogServiceExtensions
    {
        public static async Task PromptForReload(this IDialogService dialogs, NavigationManager navigation, string title = "Client version outdated", string text = "Please click the button to refresh the page.")
        {
            DialogOptions options = new DialogOptions()
            {
                CloseButton = false,
                BackdropClick = false
            };

            await dialogs.ShowMessageBoxAsync(title, text, "Reload", options: options);
            navigation.NavigateTo(navigation.Uri, forceLoad: true, replace: true);
        }
    }
}
