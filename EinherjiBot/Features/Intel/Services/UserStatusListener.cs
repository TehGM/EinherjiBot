using Discord;
using Discord.WebSocket;

namespace TehGM.EinherjiBot.Intel.Services
{
    public class UserStatusListener : AutostartService
    {
        private readonly DiscordSocketClient _client;
        private readonly IUserIntelProvider _provider;
        private readonly ILogger _log;

        public UserStatusListener(DiscordSocketClient client, IUserIntelProvider provider, ILogger<UserStatusListener> log)
        {
            this._client = client;
            this._provider = provider;
            this._log = log;

            this._client.PresenceUpdated += this.PresenceUpdatedAsync;
        }

        private async Task PresenceUpdatedAsync(SocketUser user, SocketPresence oldPresence, SocketPresence newPresence)
        {
            if (!HasStatusChanged(oldPresence, newPresence))
                return;

            this._log.LogDebug("Updating intel status for user {Username} ({UserID})", user.GetUsernameWithDiscriminator(), user.Id);
            UserIntel intel = await this._provider.GetAsync(user.Id, null, base.CancellationToken).ConfigureAwait(false);
            intel.StatusHistory.ChangeStatus(newPresence.Status);
            await this._provider.UpdateHistoryAsync(intel.StatusHistory, base.CancellationToken).ConfigureAwait(false);
        }

        private static bool HasStatusChanged(IPresence oldPresence, IPresence newPresence)
        {
            if (oldPresence.Status == newPresence.Status)
                return false;
            if (oldPresence.Status.IsOnlineStatus() && newPresence.Status.IsOnlineStatus())
                return false;
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();

            try { this._client.PresenceUpdated -= this.PresenceUpdatedAsync; } catch { }
        }
    }
}
