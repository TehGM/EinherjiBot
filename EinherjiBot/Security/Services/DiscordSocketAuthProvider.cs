using Discord;
using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.Security.Services
{
    public class DiscordSocketAuthProvider : IDiscordAuthProvider, IAuthProvider, IDisposable
    {
        public IDiscordAuthContext User { get; set; } = DiscordSocketAuthContext.None;
        IAuthContext IAuthProvider.User => this.User;

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

        public async Task<IDiscordAuthContext> GetAsync(ulong userID, ulong? guildID, CancellationToken cancellationToken = default)
        {
            Task<UserSecurityData> dataTask = this.GetUserSecurityDataAsync(userID, cancellationToken);
            Task<IUser> userTask = this._client.GetUserAsync(userID, cancellationToken);
            Task<IReadOnlyCollection<IGuild>> guildsTask = this._client.GetGuildsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions());
            await Task.WhenAll(dataTask, userTask, guildsTask).ConfigureAwait(false);

            Task<RecognizedIDs> knownIDsTask = this.GetRecognizedIDsAsync(userID, guildsTask.Result, cancellationToken);
            IGuild guild = guildID != null
                ? guildsTask.Result.FirstOrDefault(g => g.Id == guildID)
                : null;
            Task<IGuildUser> guildUserTask = guild?.GetGuildUserAsync(userID, cancellationToken) ?? Task.FromResult((IGuildUser)null);
            await Task.WhenAll(knownIDsTask, guildUserTask).ConfigureAwait(false);

            return new DiscordSocketAuthContext(userTask.Result, guild, guildUserTask.Result, knownIDsTask.Result.KnownGuildIDs, knownIDsTask.Result.KnownRoleIDs, dataTask.Result);
        }

        public async Task<IDiscordAuthContext> GetBotContextAsync(CancellationToken cancellationToken = default)
        {
            IUser user = this._client.CurrentUser;
            IEnumerable<IGuild> guilds = await this._client.GetGuildsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            RecognizedIDs recognizedIDs = await this.GetRecognizedIDsAsync(user.Id, guilds, cancellationToken).ConfigureAwait(false);

            UserSecurityData securityData = new UserSecurityData(user.Id);
            securityData.Roles.Add(UserRole.EinherjiBot);

            return new DiscordSocketAuthContext(user, null, null, recognizedIDs.KnownGuildIDs, recognizedIDs.KnownRoleIDs, securityData);
        }

        private async Task<RecognizedIDs> GetRecognizedIDsAsync(ulong userID, IEnumerable<IGuild> guilds, CancellationToken cancellationToken)
        {
            List<ulong> guildIDs = new List<ulong>(guilds.Count());
            List<ulong> roleIDs = new List<ulong>();
            foreach (IGuild guild in guilds)
            {
                IGuildUser user = await guild.GetGuildUserAsync(userID, cancellationToken).ConfigureAwait(false);
                if (user == null)
                    continue;
                guildIDs.Add(guild.Id);
                roleIDs.AddRange(user.RoleIds);
            }
            return new RecognizedIDs(guildIDs, roleIDs);
        }

        private record RecognizedIDs(IEnumerable<ulong> KnownGuildIDs, IEnumerable<ulong> KnownRoleIDs);

        public async Task<UserSecurityData> GetUserSecurityDataAsync(ulong userID, CancellationToken cancellationToken = default)
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
