using Discord;
using Discord.Interactions;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.Auditing.SharedAccounts;

namespace TehGM.EinherjiBot.SharedAccounts.Commands
{
    [Group("netflix", "Netflix-related commands")]
    public class NetflixSharedAccountSlashCommands : EinherjiInteractionModule
    {
        [Group("account", "Gives access to a shared Netflix account")]
        public class AccountCommands : EinherjiInteractionModule
        {
            private readonly ISharedAccountProvider _provider;
            private readonly ISharedAccountImageProvider _imageProvider;
            private readonly IDiscordAuthorizationService _auth;
            private readonly IAuditStore<SharedAccountAuditEntry> _audit;

            public AccountCommands(ISharedAccountProvider provider, ISharedAccountImageProvider imageProvider, 
                IDiscordAuthorizationService auth, IAuditStore<SharedAccountAuditEntry> audit)
            {
                this._provider = provider;
                this._imageProvider = imageProvider;
                this._auth = auth;
                this._audit = audit;
            }

            [SlashCommand("get", "Gets netflix account credentials", runMode: RunMode.Sync)]
            public async Task CmdGetAsync(
                [Summary("Account", "Account to get credentials for"), Autocomplete(typeof(NetflixSharedAccountAutocompleteHandler))] Guid id)
            {
                SharedAccount account = await this._provider.GetAsync(id, base.CancellationToken).ConfigureAwait(false);
                if (account == null)
                {
                    await base.RespondAsync($"{EinherjiEmote.FailureSymbol} Requested shared account not found.", ephemeral: true, options: base.GetRequestOptions());
                    return;
                }

                await this._audit.AddAuditAsync(SharedAccountAuditEntry.Retrieved(base.Context.User.Id, account.ID, base.Context.Interaction.CreatedAt.UtcDateTime));
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

                SharedAccount account = await this._provider.GetAsync(id, base.CancellationToken).ConfigureAwait(false);
                if (account == null)
                {
                    await base.RespondAsync($"{EinherjiEmote.FailureSymbol} Requested shared account not found.", ephemeral: true, options: base.GetRequestOptions());
                    return;
                }
                DiscordAuthorizationResult authorization = await this._auth.AuthorizeAsync(account, 
                    new[] { typeof(Policies.CanAccessSharedAccount), typeof(Policies.CanEditSharedAccount) }, base.CancellationToken).ConfigureAwait(false);
                if (!authorization.Succeeded)
                {
                    await base.RespondAsync($"{EinherjiEmote.FailureSymbol} {authorization.Reason}", ephemeral: true, options: base.GetRequestOptions());
                    return;
                }

                email = email?.Trim();
                password = password?.Trim();

                bool anythingChanged = false;
                if (changingEmail && account.Login != email)
                {
                    account.Login = email;
                    anythingChanged = true;
                }
                if (changingPassword && account.Password != password)
                {
                    account.Password = password;
                    anythingChanged = true;
                }

                if (!anythingChanged)
                {
                    await base.RespondAsync($"{EinherjiEmote.FacepalmOutline} Nothing to change.", ephemeral: true, options: base.GetRequestOptions());
                    return;
                }

                account.ModifiedByID = base.Context.User.Id;
                account.ModifiedTimestamp = base.Context.Interaction.CreatedAt.UtcDateTime;
                await this._provider.AddOrUpdateAsync(account, base.CancellationToken).ConfigureAwait(false);

                await this._audit.AddAuditAsync(SharedAccountAuditEntry.Updated(base.Context.User.Id, account.ID, base.Context.Interaction.CreatedAt.UtcDateTime), base.CancellationToken).ConfigureAwait(false);
                EmbedBuilder embed = await this.CreateAccountEmbedAsync(account).ConfigureAwait(false);
                await base.RespondAsync(null, embed.Build()).ConfigureAwait(false);
            }


            private async Task<EmbedBuilder> CreateAccountEmbedAsync(SharedAccount account)
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
