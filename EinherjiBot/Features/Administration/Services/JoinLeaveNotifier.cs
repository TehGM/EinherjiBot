using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.Auditing.Administration;
using TehGM.EinherjiBot.PlaceholdersEngine;

namespace TehGM.EinherjiBot.Administration.Services
{
    public class JoinLeaveNotifier : ScopedAutostartService
    {
        private readonly DiscordSocketClient _client;
        private readonly IAuditStore<JoinLeaveAuditEntry> _audit;
        private readonly ILogger _log;

        public JoinLeaveNotifier(DiscordSocketClient client, IAuditStore<JoinLeaveAuditEntry> audit, ILogger<JoinLeaveNotifier> log, IServiceScopeFactory services)
            : base(services)
        {
            this._client = client;
            this._audit = audit;
            this._log = log;

            this._client.UserJoined += this.OnUserJoinedAsync;
            this._client.UserLeft += this.OnUserLeftAsync;
        }

        private async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            IGuild guild = user.Guild;
            this._log.LogDebug("User {User} ({UserID}) joined guild {Guild} ({GuildID})", user.GetUsernameWithDiscriminator(), user.Id, guild.Name, guild.Id);
            await this._audit.AddAuditAsync(JoinLeaveAuditEntry.UserJoined(user.Id, guild.Id, user.JoinedAt.Value.UtcDateTime), base.CancellationToken).ConfigureAwait(false);

            using IServiceScope scope = await base.CreateBotUserScopeAsync(base.CancellationToken).ConfigureAwait(false);
            IGuildSettingsHandler settingsHandler = scope.ServiceProvider.GetRequiredService<IGuildSettingsHandler>();
            IGuildSettings settings = await settingsHandler.GetAsync(guild.Id, base.CancellationToken).ConfigureAwait(false);

            await this.SendNotificationAsync(settings.JoinNotification, user.Guild, user).ConfigureAwait(false);
        }

        protected async Task OnUserLeftAsync(SocketGuild guild, SocketUser user)
        {
            this._log.LogDebug("User {User} ({UserID}) left guild {Guild} ({GuildID})", user.GetUsernameWithDiscriminator(), user.Id, guild.Name, guild.Id);
            await this._audit.AddAuditAsync(JoinLeaveAuditEntry.UserLeft(user.Id, guild.Id, DateTime.UtcNow), base.CancellationToken).ConfigureAwait(false);

            using IServiceScope scope = await base.CreateBotUserScopeAsync(base.CancellationToken).ConfigureAwait(false);
            IGuildSettingsHandler settingsHandler = scope.ServiceProvider.GetRequiredService<IGuildSettingsHandler>();
            IGuildSettings settings = await settingsHandler.GetAsync(guild.Id, base.CancellationToken).ConfigureAwait(false);

            await this.SendNotificationAsync(settings.LeaveNotification, guild, user).ConfigureAwait(false);
        }

        private static readonly Regex _placeholderCheckRegex = new Regex(@"{{.+}}", RegexOptions.Singleline);
        private async Task SendNotificationAsync(IJoinLeaveSettings settings, IGuild guild, IUser user)
        {
            if (settings == null || !settings.IsEnabled)
                return;
            if (string.IsNullOrEmpty(settings.MessageTemplate))
                return;
            if (!settings.UseSystemChannel && settings.NotificationChannelID == null)
                return;

            ulong? channelID = settings.UseSystemChannel ? guild.SystemChannelId : settings.NotificationChannelID;
            if (channelID == null)
                return;
            ITextChannel channel = await guild.GetTextChannelAsync(channelID.Value, CacheMode.AllowDownload, base.CancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (channel == null)
                return;

            IGuildUser currentUser = await guild.GetGuildUserAsync(this._client.CurrentUser.Id, base.CancellationToken).ConfigureAwait(false);
            ChannelPermissions permissions = currentUser.GetPermissions(channel);
            if (!permissions.SendMessages)
            {
                this._log.LogDebug("No permissions to post in {Guild}'s ({GuildID}) channel {Channel} ({ChannelID}), skipping notification", guild.Name, guild.Id, channel.Name, channel.Id);
                return;
            }

            try
            {
                string text = await GetTextAsync();
                EmbedBuilder embed = new EmbedBuilder()
                    .WithDescription(text)
                    .WithColor(settings.EmbedColor);
                if (settings.ShowUserAvatar)
                    embed.WithThumbnailUrl(user.GetSafeAvatarUrl());
                await channel.SendMessageAsync(null, false, embed.Build(), base.CancellationToken.ToRequestOptions()).ConfigureAwait(false);
            }
            catch (PlaceholderException ex)
            {
                return;
            }

            // to avoid unnecessarily requesting auth context, running placeholders engine etc, check if template even contains a placeholder
            async Task<string> GetTextAsync()
            {
                bool containsPlaceholders = _placeholderCheckRegex.IsMatch(settings.MessageTemplate);
                if (!containsPlaceholders)
                    return settings.MessageTemplate;

                using IServiceScope scope = base.CreateScope();
                IDiscordAuthProvider authProvider = scope.ServiceProvider.GetRequiredService<IDiscordAuthProvider>();
                IDiscordAuthContext authContext = await authProvider.GetAsync(user.Id, guild.Id, base.CancellationToken).ConfigureAwait(false);
                authProvider.User = authContext;

                IPlaceholdersEngine engine = scope.ServiceProvider.GetRequiredService<IPlaceholdersEngine>();
                return await engine.ConvertPlaceholdersAsync(settings.MessageTemplate, PlaceholderUsage.GuildEvent | PlaceholderUsage.UserEvent, base.CancellationToken).ConfigureAwait(false);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            try { this._client.UserLeft -= this.OnUserLeftAsync; } catch { }
            try { this._client.UserJoined -= this.OnUserJoinedAsync; } catch { }
        }
    }
}
