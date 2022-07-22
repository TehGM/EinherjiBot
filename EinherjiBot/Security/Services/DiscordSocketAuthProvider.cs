using Discord;
using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.Security.Services
{
    public class DiscordSocketAuthProvider : IAuthProvider, IDisposable
    {
        public IAuthContext Current { get; set; }

        private readonly IDiscordClient _client;
        private readonly IUserSecurityDataStore _store;
        private readonly IEntityCache<ulong, UserSecurityData> _userDataCache;
        private readonly ILogger _log;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public DiscordSocketAuthProvider(IDiscordClient client, IUserSecurityDataStore store, 
            IEntityCache<ulong, UserSecurityData> userDataCache, ILogger<DiscordSocketAuthProvider> log)
        {
            this._client = client;
            this._store = store;
            this._userDataCache = userDataCache;
            this._log = log;

            this._userDataCache.DefaultExpiration = new TimeSpanEntityExpiration(TimeSpan.FromMinutes(5));
        }

        public async Task<IAuthContext> GetAsync(ulong userID, ulong? guildID, CancellationToken cancellationToken = default)
        {
            Task<UserSecurityData> dataTask = this.GetUserSecurityDataAsync(userID, cancellationToken);
            Task<IUser> userTask = this._client.GetUserAsync(userID, cancellationToken);
            Task<IReadOnlyCollection<IGuild>> guildsTask = this._client.GetGuildsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions());
            await Task.WhenAll(dataTask, userTask, guildsTask).ConfigureAwait(false);

            Task<IEnumerable<ulong>> knownRolesTask = this.GetKnownRoleIDsAsync(userID, guildsTask.Result, cancellationToken);
            IGuild guild = guildID != null
                ? guildsTask.Result.FirstOrDefault(g => g.Id == guildID)
                : null;
            Task<IGuildUser> guildUserTask = guild?.GetGuildUserAsync(userID, cancellationToken) ?? Task.FromResult((IGuildUser)null);

            return new DiscordSocketAuthContext(userTask.Result, guild, guildUserTask.Result, knownRolesTask.Result, dataTask.Result);
        }

        private async Task<IEnumerable<ulong>> GetKnownRoleIDsAsync(ulong userID, IEnumerable<IGuild> guilds, CancellationToken cancellationToken)
        {
            List<ulong> results = new List<ulong>();
            foreach (IGuild guild in guilds)
            {
                IGuildUser user = await guild.GetGuildUserAsync(userID, cancellationToken).ConfigureAwait(false);
                if (user == null)
                    continue;
                results.AddRange(user.RoleIds);
            }
            return results;
        }

        private async Task<UserSecurityData> GetUserSecurityDataAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                UserSecurityData result = this._userDataCache.Get(userID);
                if (result != null)
                {
                    this._log.LogTrace("User security data for user {UserID} found in cache", userID);
                    return result;
                }

                result = await this._store.GetAsync(userID, cancellationToken).ConfigureAwait(false);

                if (result == null)
                {
                    this._log.LogTrace("User intel for user {UserID} not found, creating new with defaults", userID);
                    result = new UserSecurityData(userID);
                }

                this._userDataCache.AddOrReplace(result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public void Dispose()
        {
            try { this._lock?.Dispose(); } catch { }
        }
    }
}
