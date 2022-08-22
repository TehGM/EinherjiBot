﻿using MudBlazor;
using TehGM.EinherjiBot.PlaceholdersEngine;
using TehGM.EinherjiBot.UI.Components.PlaceholdersEngine;

namespace TehGM.EinherjiBot.UI.PlaceholdersEngine.Services
{
    public class PlaceholdersBuilderOpener : IPlaceholdersBuilder
    {
        private readonly IDialogService _dialogService;

        public PlaceholdersBuilderOpener(IDialogService dialogService)
        {
            this._dialogService = dialogService;
        }

        public async Task<PlaceholderBuilderResult> OpenAsync(PlaceholderUsage context)
        {
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
