using Discord;
using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.Intel.Services
{
    public class UserIntelProvider : IUserIntelProvider
    {
        private readonly IDiscordClient _client;
        private readonly IEntityCache<ulong, UserOnlineHistory> _historyCache;
        private readonly IUserOnlineHistoryStore _historyStore;
        private readonly ILogger _log;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public UserIntelProvider(IDiscordClient client, ILogger<UserIntelProvider> log, 
            IEntityCache<ulong, UserOnlineHistory> historyCache, IUserOnlineHistoryStore historyStore)
        {
            this._client = client;
            this._historyCache = historyCache;
            this._log = log;
            this._historyStore = historyStore;
        }

        public async Task<UserIntel> GetAsync(ulong userID, ulong? guildID, CancellationToken cancellationToken = default)
        {
            Task<UserOnlineHistory> historyTask = this.GetOnlineHistoryAsync(userID, cancellationToken);
            Task<IUser> userTask = this._client.GetUserAsync(userID, cancellationToken);
            Task<IGuildUser> guildUserTask = Task.Run(async () =>
            {
                if (guildID != null)
                {
                    IGuild guild = await this._client.GetGuildAsync(guildID.Value, cancellationToken).ConfigureAwait(false);
                    return await guild.GetGuildUserAsync(userID, cancellationToken).ConfigureAwait(false);
                }
                else
                    return null;
            }, cancellationToken);

            await Task.WhenAll(historyTask, userTask, guildUserTask).ConfigureAwait(false);

            return new UserIntel(userTask.Result, guildUserTask.Result, historyTask.Result);
        }

        private async Task<UserOnlineHistory> GetOnlineHistoryAsync(ulong userID, CancellationToken cancellationToken)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                UserOnlineHistory result = this._historyCache.Get(userID);
                if (result != null)
                {
                    this._log.LogTrace("User intel for user {UserID} found in cache", userID);
                    return result;
                }

                result = await this._historyStore.GetAsync(userID, cancellationToken).ConfigureAwait(false);

                if (result == null)
                {
                    this._log.LogTrace("User intel for user {UserID} not found, creating new with defaults", userID);
                    result = new UserOnlineHistory(userID);
                }

                this._historyCache.AddOrReplace(result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task UpdateHistoryAsync(UserOnlineHistory intel, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this._log.LogTrace("Updating user intel for user {UserID}", intel.ID);
                await this._historyStore.UpdateAsync(intel, cancellationToken).ConfigureAwait(false);
                this._historyCache.AddOrReplace(intel);
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}
