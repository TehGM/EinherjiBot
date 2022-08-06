using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace TehGM.EinherjiBot.Intel.Services
{
    public class UserStatusListener : ScopedAutostartService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _log;

        public UserStatusListener(DiscordSocketClient client, ILogger<UserStatusListener> log, IServiceScopeFactory services)
            : base(services)
        {
            this._client = client;
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

            using IServiceScope scope = await base.CreateBotUserScopeAsync(base.CancellationToken).ConfigureAwait(false);
            IUserIntelProvider provider = scope.ServiceProvider.GetRequiredService<IUserIntelProvider>();
            UserIntelContext intel = await provider.GetAsync(user.Id, null, base.CancellationToken).ConfigureAwait(false);

            if (hasStatusChanged)
                hasStatusChanged = intel.Intel.ChangeStatus(newPresence.Status);
            if (hasCustomStatusChanged)
                hasCustomStatusChanged = intel.Intel.ChangeCustomStatus(GetCustomStatus(newPresence));
            if (hasSpotifyStatusChanged)
                hasSpotifyStatusChanged = intel.Intel.ChangeListeningStatus(GetSpotifyStatus(newPresence));
            if (HasAnythingChanged())
            {
                this._log.LogDebug("Updating intel status for user {Username} ({UserID})", user.GetUsernameWithDiscriminator(), user.Id);
                await provider.UpdateIntelAsync(intel.Intel, base.CancellationToken).ConfigureAwait(false);
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
            => newPresence.Status.IsOnlineStatus() && GetCustomStatus(oldPresence) != GetCustomStatus(newPresence);
        private static bool HasSpotifyStatusChanged(IPresence oldPresence, IPresence newPresence)
        {
            SpotifyGame newStatus = GetSpotifyStatus(newPresence);
            return newStatus != null && GetSpotifyStatus(oldPresence) != newStatus;
        }  

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
