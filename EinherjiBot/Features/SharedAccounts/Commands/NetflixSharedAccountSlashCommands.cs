using Discord;
using Discord.Interactions;

namespace TehGM.EinherjiBot.SharedAccounts.Commands
{
    [Group("netflix", "Netflix-related commands")]
    public class NetflixSharedAccountSlashCommands : EinherjiInteractionModule
    {
        [Group("account", "Gives access to a shared Netflix account")]
        public class AccountCommands : EinherjiInteractionModule
        {
            private readonly ISharedAccountHandler _handler;
            private readonly ISharedAccountImageProvider _imageProvider;

            public AccountCommands(ISharedAccountHandler handler, ISharedAccountImageProvider imageProvider)
            {
                this._handler = handler;
                this._imageProvider = imageProvider;
            }

            [SlashCommand("get", "Gets netflix account credentials", runMode: RunMode.Sync)]
            public async Task CmdGetAsync(
                [Summary("Account", "Account to get credentials for"), Autocomplete(typeof(NetflixSharedAccountAutocompleteHandler))] Guid id)
            {
                ISharedAccount account = await this._handler.GetAsync(id, base.CancellationToken).ConfigureAwait(false);
                if (account == null)
                {
                    await base.RespondAsync($"{EinherjiEmote.FailureSymbol} Requested shared account not found.", ephemeral: true, options: base.GetRequestOptions());
                    return;
                }

                EmbedBuilder embed = await this.CreateAccountEmbedAsync(account).ConfigureAwait(false);
                await base.RespondAsync(embed: embed.Build(), ephemeral: true, options: base.GetRequestOptions()).ConfigureAwait(false);
            }

            [SlashCommand("update", "Updates netflix account credentials")]
            public async Task CmdUpdateAsync(
                [Summary("Account", "Account to update credentials for"), Autocomplete(typeof(NetflixSharedAccountModerationAutocompleteHandler))] Guid id,
                [Summary("Email", "New email value")] string email = null,
                [Summary("Password", "New password value")] string password = null)
            {
                bool changingEmail = !string.IsNullOrWhiteSpace(email);
                bool changingPassword = !string.IsNullOrWhiteSpace(password);
                if (!changingEmail && !changingPassword)
                {
                    await base.RespondAsync($"{EinherjiEmote.FailureSymbol} Please provide data to change.", ephemeral: true, options: base.GetRequestOptions());
                    return;
                }

                ISharedAccount account = await this._handler.GetAsync(id, base.CancellationToken).ConfigureAwait(false);
                if (account == null)
                {
                    await base.RespondAsync($"{EinherjiEmote.FailureSymbol} Requested shared account not found.", ephemeral: true, options: base.GetRequestOptions());
                    return;
                }

                SharedAccountRequest request = SharedAccountRequest.FromAccount(account);
                request.Login = changingEmail ? email?.Trim() : account.Login;
                request.Password = changingPassword ? password?.Trim() : account.Password;
                if (!account.HasChanges(request))
                {
                    await base.RespondAsync($"{EinherjiEmote.FacepalmOutline} Nothing to change.", ephemeral: true, options: base.GetRequestOptions());
                    return;
                }

                IEntityUpdateResult<ISharedAccount> result = await this._handler.UpdateAsync(account.ID, request, base.CancellationToken).ConfigureAwait(false);
                if (result.Saved())
                {
                    EmbedBuilder embed = await this.CreateAccountEmbedAsync(result.Entity).ConfigureAwait(false);
                    await base.RespondAsync(null, embed.Build()).ConfigureAwait(false);
                }
                else if (result.NoChanges())
                    await base.RespondAsync($"{EinherjiEmote.FacepalmOutline} Nothing to change.", ephemeral: true, options: base.GetRequestOptions());
            }


            private async Task<EmbedBuilder> CreateAccountEmbedAsync(ISharedAccount account)
            {
                EmbedBuilder embed = new EmbedBuilder()
                    .AddField("Login", account.Login)
                    .AddField("Password", account.Password)
                    .WithColor(EinherjiColor.SuccessColor)
                    .WithThumbnailUrl(await this._imageProvider.GetAccountImageUrlAsync(account.AccountType));
                if (account.ModifiedByID != null)
                {
                    IUser user = await base.Context.Client.GetUserAsync(account.ModifiedByID.Value, base.CancellationToken).ConfigureAwait(false);
                    embed
                        .WithTimestamp(account.ModifiedTimestamp.Value)
                        .WithFooter($"Last modified by {user.GetUsernameWithDiscriminator()}", user.GetSafeAvatarUrl());
                }
                return embed;
            }
        }
    }
}
