using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.Auditing.Administration;

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
            if (settings.JoinNotificationChannelID != null)
            {
                ITextChannel channel = await guild.GetTextChannelAsync(settings.JoinNotificationChannelID.Value, CacheMode.AllowDownload, base.CancellationToken.ToRequestOptions()).ConfigureAwait(false);
                if (channel == null)
                    return;

                IGuildUser currentUser = await guild.GetGuildUserAsync(this._client.CurrentUser.Id, base.CancellationToken).ConfigureAwait(false);
                ChannelPermissions permissions = currentUser.GetPermissions(channel);
                if (!permissions.SendMessages)
                {
                    this._log.LogDebug("No permissions to post in {Guild}'s ({GuildID}) channel {Channel} ({ChannelID}), skipping notification", guild.Name, guild.Id, channel.Name, channel.Id);
                    return;
                }

                EmbedBuilder embed = new EmbedBuilder()
                    .WithDescription($"**{user.Mention}** *(`{user.GetUsernameWithDiscriminator()}`)* **has joined.**")
                    .WithColor((Color)System.Drawing.Color.Cyan);
                await channel.SendMessageAsync(null, false, embed.Build(), base.CancellationToken.ToRequestOptions()).ConfigureAwait(false);
            }
        }

        protected async Task OnUserLeftAsync(SocketGuild guild, SocketUser user)
        {
            this._log.LogDebug("User {User} ({UserID}) left guild {Guild} ({GuildID})", user.GetUsernameWithDiscriminator(), user.Id, guild.Name, guild.Id);
            await this._audit.AddAuditAsync(JoinLeaveAuditEntry.UserLeft(user.Id, guild.Id, DateTime.UtcNow), base.CancellationToken).ConfigureAwait(false);

            using IServiceScope scope = await base.CreateBotUserScopeAsync(base.CancellationToken).ConfigureAwait(false);
            IGuildSettingsHandler settingsHandler = scope.ServiceProvider.GetRequiredService<IGuildSettingsHandler>();
            IGuildSettings settings = await settingsHandler.GetAsync(guild.Id, base.CancellationToken).ConfigureAwait(false);
            if (settings.LeaveNotificationChannelID != null)
            {
                ITextChannel channel = guild.GetTextChannel(settings.LeaveNotificationChannelID.Value);
                if (channel == null)
                    return;

                IGuildUser currentUser = await guild.GetGuildUserAsync(this._client.CurrentUser.Id, base.CancellationToken).ConfigureAwait(false);
                ChannelPermissions permissions = currentUser.GetPermissions(guild.SystemChannel);
                if (!permissions.SendMessages)
                {
                    this._log.LogDebug("No permissions to post in {Guild}'s ({GuildID}) channel {Channel} ({ChannelID}),, skipping notification", guild.Name, guild.Id, channel.Name, channel.Id);
                    return;
                }

                EmbedBuilder embed = new EmbedBuilder()
                    .WithDescription($"**{user.Mention}** *(`{user.GetUsernameWithDiscriminator()}`)* **has left.**")
                    .WithColor((Color)System.Drawing.Color.Cyan);
                await channel.SendMessageAsync(null, false, embed.Build(), base.CancellationToken.ToRequestOptions()).ConfigureAwait(false);
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
