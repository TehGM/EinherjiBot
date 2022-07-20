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

        protected Task OnUserLeftAsync(SocketGuild guild, SocketUser user)
        {
            this._log.LogDebug("User {User} ({UserID}) left guild {Guild} ({GuildID})", user.GetUsernameWithDiscriminator(), user.Id, guild.Name, guild.Id);
            if (guild.SystemChannel == null)
                return Task.CompletedTask;
            EmbedBuilder embed = new EmbedBuilder()
                .WithDescription($"**{user.Mention}** *(`{user.Username}#{user.Discriminator}`)* **has left.**")
                .WithColor((Color)System.Drawing.Color.Cyan);
            return guild.SystemChannel.SendMessageAsync(null, false, embed.Build(), base.CancellationToken);
        }

        public override void Dispose()
        {
            base.Dispose();

            try { this._client.UserLeft -= this.OnUserLeftAsync; } catch { }
        }
    }
}
