using Discord;
using Discord.WebSocket;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.Auditing.Administration;

namespace TehGM.EinherjiBot.Administration.Services
{
    public class JoinLeaveNotifier : AutostartService
    {
        private readonly DiscordSocketClient _client;
        private readonly IAuditStore<JoinLeaveAuditEntry> _audit;
        private readonly ILogger _log;

        public JoinLeaveNotifier(DiscordSocketClient client, IAuditStore<JoinLeaveAuditEntry> audit, ILogger<JoinLeaveNotifier> log)
        {
            this._client = client;
            this._audit = audit;
            this._log = log;

            this._client.UserJoined += this.OnUserJoined;
            this._client.UserLeft += this.OnUserLeftAsync;
        }

        private Task OnUserJoined(SocketGuildUser user)
        {
            IGuild guild = user.Guild;
            this._log.LogDebug("User {User} ({UserID}) joined guild {Guild} ({GuildID})", user.GetUsernameWithDiscriminator(), user.Id, guild.Name, guild.Id);
            return this._audit.AddAuditAsync(JoinLeaveAuditEntry.UserJoined(user.Id, guild.Id, user.JoinedAt.Value.UtcDateTime), base.CancellationToken);
        }

        protected async Task OnUserLeftAsync(SocketGuild guild, SocketUser user)
        {
            this._log.LogDebug("User {User} ({UserID}) left guild {Guild} ({GuildID})", user.GetUsernameWithDiscriminator(), user.Id, guild.Name, guild.Id);
            if (guild.SystemChannel == null)
            {
                this._log.LogDebug("{Guild} ({GuildID}) has no system channel, skipping notification", guild.Name, guild.Id);
                return;
            }

            IGuildUser currentUser = await guild.GetGuildUserAsync(this._client.CurrentUser.Id, base.CancellationToken).ConfigureAwait(false);
            ChannelPermissions permissions = currentUser.GetPermissions(guild.SystemChannel);
            if (!permissions.SendMessages)
            {
                this._log.LogDebug("No permissions to post in {Guild}'s ({GuildID}) system channel, skipping notification", guild.Name, guild.Id);
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithDescription($"**{user.Mention}** *(`{user.GetUsernameWithDiscriminator()}`)* **has left.**")
                .WithColor((Color)System.Drawing.Color.Cyan);
            await guild.SystemChannel.SendMessageAsync(null, false, embed.Build(), base.CancellationToken).ConfigureAwait(false);
            await this._audit.AddAuditAsync(JoinLeaveAuditEntry.UserLeft(user.Id, guild.Id, DateTime.UtcNow), base.CancellationToken).ConfigureAwait(false);
        }

        public override void Dispose()
        {
            base.Dispose();

            try { this._client.UserLeft -= this.OnUserLeftAsync; } catch { }
            try { this._client.UserJoined -= this.OnUserJoined; } catch { }
        }
    }
}
