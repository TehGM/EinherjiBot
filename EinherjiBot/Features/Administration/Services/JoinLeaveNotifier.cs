using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.Auditing.Administration;
using TehGM.EinherjiBot.PlaceholdersEngine;
using TehGM.EinherjiBot.UI.Pages.Bot;

namespace TehGM.EinherjiBot.Administration.Services
{
    public class JoinLeaveNotifier : ScopedAutostartService
    {
        private readonly DiscordSocketClient _client;
        private readonly IGuildSettingsProvider _provider;
        private readonly IAuditStore<JoinLeaveAuditEntry> _audit;
        private readonly ILogger _log;

        public JoinLeaveNotifier(DiscordSocketClient client, IGuildSettingsProvider provider, 
            IAuditStore<JoinLeaveAuditEntry> audit, ILogger<JoinLeaveNotifier> log, IServiceScopeFactory services)
            : base(services)
        {
            this._client = client;
            this._provider = provider;
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

            GuildSettings settings = await this._provider.GetAsync(guild.Id, base.CancellationToken).ConfigureAwait(false);

            await this.SendNotificationAsync(settings, settings.JoinNotification, user.Guild, user).ConfigureAwait(false);
        }

        protected async Task OnUserLeftAsync(SocketGuild guild, SocketUser user)
        {
            this._log.LogDebug("User {User} ({UserID}) left guild {Guild} ({GuildID})", user.GetUsernameWithDiscriminator(), user.Id, guild.Name, guild.Id);
            await this._audit.AddAuditAsync(JoinLeaveAuditEntry.UserLeft(user.Id, guild.Id, DateTime.UtcNow), base.CancellationToken).ConfigureAwait(false);

            GuildSettings settings = await this._provider.GetAsync(guild.Id, base.CancellationToken).ConfigureAwait(false);

            await this.SendNotificationAsync(settings, settings.LeaveNotification, guild, user).ConfigureAwait(false);
        }

        private static readonly Regex _placeholderCheckRegex = new Regex(@"{{.+}}", RegexOptions.Singleline);
        private async Task SendNotificationAsync(GuildSettings guildSettings, JoinLeaveSettings notifSettings, IGuild guild, IUser user)
        {
            if (notifSettings == null || !notifSettings.IsEnabled)
                return;
            if (string.IsNullOrEmpty(notifSettings.MessageTemplate))
            {
                await SaveErrorAsync("Message template is empty.").ConfigureAwait(false);
                return;
            }
            if (!notifSettings.UseSystemChannel && notifSettings.NotificationChannelID == null)
            {
                await SaveErrorAsync("No channel configured.").ConfigureAwait(false);
                return;
            }

            ulong? channelID = notifSettings.UseSystemChannel ? guild.SystemChannelId : notifSettings.NotificationChannelID;
            if (channelID == null)
            {
                if (notifSettings.UseSystemChannel)
                    await SaveErrorAsync("Guild has no system channel.").ConfigureAwait(false);
                return;
            }
            ITextChannel channel = await guild.GetTextChannelAsync(channelID.Value, CacheMode.AllowDownload, base.CancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (channel == null)
            {
                await SaveErrorAsync($"Channel {channelID} not found.").ConfigureAwait(false);
                return;
            }

            IGuildUser currentUser = await guild.GetGuildUserAsync(this._client.CurrentUser.Id, base.CancellationToken).ConfigureAwait(false);
            ChannelPermissions permissions = currentUser.GetPermissions(channel);
            if (!permissions.SendMessages)
            {
                this._log.LogDebug("No permissions to post in {Guild}'s ({GuildID}) channel {Channel} ({ChannelID}), skipping notification", guild.Name, guild.Id, channel.Name, channel.Id);
                await SaveErrorAsync($"{EinherjiInfo.Name} has no permissions to post in {guild.Name}'s channel {channel.Name}.").ConfigureAwait(false);
                return;
            }

            try
            {
                string text = await GetTextAsync();
                EmbedBuilder embed = new EmbedBuilder()
                    .WithDescription(text)
                    .WithColor(notifSettings.EmbedColor);
                if (notifSettings.ShowUserAvatar)
                    embed.WithThumbnailUrl(user.GetSafeAvatarUrl());
                await channel.SendMessageAsync(null, false, embed.Build(), base.CancellationToken.ToRequestOptions()).ConfigureAwait(false);
            }
            catch (PlaceholderException ex)
            {
                await SaveErrorAsync(ex.Message).ConfigureAwait(false);
                return;
            }

            // to avoid unnecessarily requesting auth context, running placeholders engine etc, check if template even contains a placeholder
            async Task<string> GetTextAsync()
            {
                bool containsPlaceholders = _placeholderCheckRegex.IsMatch(notifSettings.MessageTemplate);
                if (!containsPlaceholders)
                    return notifSettings.MessageTemplate;

                using IServiceScope scope = base.CreateScope();
                IDiscordAuthProvider authProvider = scope.ServiceProvider.GetRequiredService<IDiscordAuthProvider>();
                IDiscordAuthContext authContext = await authProvider.GetAsync(user.Id, guild.Id, base.CancellationToken).ConfigureAwait(false);
                authProvider.User = authContext;

                IPlaceholdersEngine engine = scope.ServiceProvider.GetRequiredService<IPlaceholdersEngine>();
                return await engine.ConvertPlaceholdersAsync(notifSettings.MessageTemplate, PlaceholderUsage.GuildEvent | PlaceholderUsage.UserEvent, base.CancellationToken).ConfigureAwait(false);
            }

            Task SaveErrorAsync(string error)
            {
                notifSettings.LastError = new ErrorInfo(DateTime.UtcNow, error);
                return this._provider.AddOrUpdateAsync(guildSettings, base.CancellationToken);
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
