using Discord;
using Discord.WebSocket;

namespace TehGM.EinherjiBot.Features.Administration
{
    public class UserLeaveNotifier : AutostartService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _log;

        public UserLeaveNotifier(DiscordSocketClient client, ILogger<UserLeaveNotifier> log)
        {
            this._client = client;
            this._log = log;

            this._client.UserLeft += this.OnUserLeftAsync;
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
        }

        public override void Dispose()
        {
            base.Dispose();

            try { this._client.UserLeft -= this.OnUserLeftAsync; } catch { }
        }
    }
}
