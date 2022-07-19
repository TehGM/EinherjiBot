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
            bool hasStatusChanged = HasStatusChanged(oldPresence, newPresence);
            bool hasCustomStatusChanged = HasCustomStatusChanged(oldPresence, newPresence);
            bool hasSpotifyStatusChanged = HasSpotifyStatusChanged(oldPresence, newPresence);
            if (!HasAnythingChanged())
                return;

            UserIntelContext intel = await this._provider.GetAsync(user.Id, null, base.CancellationToken).ConfigureAwait(false);

            if (hasStatusChanged)
                hasStatusChanged = intel.Intel.ChangeStatus(newPresence.Status);
            if (hasCustomStatusChanged)
                hasCustomStatusChanged = intel.Intel.ChangeCustomStatus(GetCustomStatus(newPresence));
            if (hasSpotifyStatusChanged)
                hasSpotifyStatusChanged = intel.Intel.ChangeListeningStatus(GetSpotifyStatus(newPresence));
            if (HasAnythingChanged())
            {
                this._log.LogDebug("Updating intel status for user {Username} ({UserID})", user.GetUsernameWithDiscriminator(), user.Id);
                await this._provider.UpdateIntelAsync(intel.Intel, base.CancellationToken).ConfigureAwait(false);
            }

            bool HasAnythingChanged()
                => hasStatusChanged || hasCustomStatusChanged || hasSpotifyStatusChanged;
        }

        private static bool HasStatusChanged(IPresence oldPresence, IPresence newPresence)
        {
            if (oldPresence.Status == newPresence.Status)
                return false;
            if (oldPresence.Status.IsOnlineStatus() && newPresence.Status.IsOnlineStatus())
                return false;
            return true;
        }

        private static bool HasCustomStatusChanged(IPresence oldPresence, IPresence newPresence)
            => GetCustomStatus(oldPresence) != GetCustomStatus(newPresence);
        private static bool HasSpotifyStatusChanged(IPresence oldPresence, IPresence newPresence)
            => GetSpotifyStatus(oldPresence) != GetSpotifyStatus(newPresence);

        private static string GetCustomStatus(IPresence presence)
            => presence?.Activities?.Where(activity => activity is CustomStatusGame)
                .Cast<CustomStatusGame>()
                .FirstOrDefault()?.ToString();

        private static SpotifyGame GetSpotifyStatus(IPresence presence)
            => presence?.Activities?.Where(activity => activity is SpotifyGame)
                .Cast<SpotifyGame>()
                .FirstOrDefault();

        public override void Dispose()
        {
            base.Dispose();

            try { this._client.PresenceUpdated -= this.PresenceUpdatedAsync; } catch { }
        }
    }
}
