using System.Web;
using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.UI.API.Services
{
    public class WebDiscordEntityInfoService : IDiscordEntityInfoService
    {
        public record struct GuildUserKey(ulong UserID, ulong GuildID);

        private readonly IApiClient _client;
        private readonly IEntityCache<ulong, UserInfoResponse> _usersCache;
        private readonly IEntityCache<ulong, RoleInfoResponse> _rolesCache;
        private readonly IEntityCache<GuildUserKey, GuildUserInfoResponse> _guildUserCache;
        private readonly IEntityCache<ulong, GuildInfoResponse> _guildCache;
        private readonly ILockProvider _lock;

        private ulong? _botID = null;
        private IEnumerable<GuildInfoResponse> _cachedAllGuilds;
        private DateTime _cachedAllGuildsTimestamp;

        public WebDiscordEntityInfoService(IApiClient client, ILockProvider<WebDiscordEntityInfoService> lockProvider, 
            IEntityCache<ulong, UserInfoResponse> usersCache, IEntityCache<ulong, RoleInfoResponse> rolesCache, 
            IEntityCache<GuildUserKey, GuildUserInfoResponse> guildUserCache, IEntityCache<ulong, GuildInfoResponse> guildCache)
        {
            this._client = client;
            this._usersCache = usersCache;
            this._rolesCache = rolesCache;
            this._guildUserCache = guildUserCache;
            this._guildCache = guildCache;
            this._lock = lockProvider;
        }

        public async ValueTask<UserInfoResponse> GetBotInfoAsync(CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this._botID == null || !this._usersCache.TryGet(this._botID.Value, out UserInfoResponse cachedResult))
                {
                    UserInfoResponse result = await this._client.GetJsonAsync<UserInfoResponse>("entity-info/bot", cancellationToken).ConfigureAwait(false);
                    this._botID = result.ID;
                    this.CacheUser(result);
                    return result;
                }
                return cachedResult;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<UserInfoResponse> GetUserInfoAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this._usersCache.TryGet(userID, out UserInfoResponse result))
                    return result;

                result = await this._client.GetJsonAsync<UserInfoResponse>($"entity-info/user/{userID}", cancellationToken).ConfigureAwait(false);
                this.CacheUser(result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<IEnumerable<GuildInfoResponse>> GetGuildInfosAsync(IEnumerable<ulong> ids, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                string url = "entity-info/guilds";

                // no passed IDs means we request all that user has access to
                if (ids?.Any() != true)
                {
                    TimeSpan cacheExpiration = TimeSpan.FromMinutes(5);
                    if (this._cachedAllGuilds != null && this._cachedAllGuildsTimestamp < DateTime.UtcNow - cacheExpiration)
                        return this._cachedAllGuilds;
                    else
                    {
                        IEnumerable<GuildInfoResponse> response = await this._client.GetJsonAsync<IEnumerable<GuildInfoResponse>>(url, cancellationToken).ConfigureAwait(false);
                        this._cachedAllGuilds = response;
                        this._cachedAllGuildsTimestamp = DateTime.UtcNow;
                        foreach (GuildInfoResponse guild in response)
                            this.CacheGuild(guild);
                        return response;
                    }
                }
                else
                {
                    List<GuildInfoResponse> results = new List<GuildInfoResponse>(ids.Count());

                    // if some are cached, we don't need to retrieve them from server, eh?
                    results.AddRange(this._guildCache.Find(g => ids.Contains(g.ID)));
                    IEnumerable<ulong> remaining = ids.Except(results.Select(g => g.ID));

                    if (remaining.Any())
                    {
                        url = BuildWithArrayQuery(url, "guild", remaining);
                        IEnumerable<GuildInfoResponse> response = await this._client.GetJsonAsync<IEnumerable<GuildInfoResponse>>(url, cancellationToken).ConfigureAwait(false);
                        results.AddRange(response);
                        foreach (GuildInfoResponse guild in response)
                            this.CacheGuild(guild);
                    }

                    return results.ToArray();
                }
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<GuildUserInfoResponse> GetGuildUserInfoAsync(ulong userID, ulong guildID, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // guild user caching needs special treatment, as same user might be in different guilds
                GuildUserKey key = new GuildUserKey(userID, guildID);
                if (this._guildUserCache.TryGet(key, out GuildUserInfoResponse result))
                    return result;

                result = await this._client.GetJsonAsync<GuildUserInfoResponse>($"entity-info/guild/{guildID}/user/{userID}", cancellationToken).ConfigureAwait(false);
                this.CacheGuildUser(result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<RoleInfoResponse> GetRoleInfoAsync(ulong roleID, IEnumerable<ulong> guildIDs, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this._rolesCache.TryGet(roleID, out RoleInfoResponse result))
                    return result;

                string url = BuildWithArrayQuery($"entity-info/role/{roleID}", "guild", guildIDs);
                result = await this._client.GetJsonAsync<RoleInfoResponse>(url, cancellationToken).ConfigureAwait(false);
                this.CacheRole(result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        private void CacheGuildUser(GuildUserInfoResponse user)
        {
            // guild user caching needs special treatment, as same user might be in different guilds
            GuildUserKey key = new GuildUserKey(user.ID, user.GuildID);
            this._guildUserCache.AddOrReplace(key, user, new SlidingEntityExpiration(TimeSpan.FromMinutes(5)));

            this.CacheUser(user);
        }

        private void CacheUser(UserInfoResponse user)
        {
            TimeSpan expiration = TimeSpan.FromMinutes(5);
            if (this._botID != null && user.ID == this._botID)
                expiration = TimeSpan.FromMinutes(50);
            this._usersCache.AddOrReplace(user, new SlidingEntityExpiration(expiration));
        }

        private void CacheRole(RoleInfoResponse role)
        {
            this._rolesCache.AddOrReplace(role, new SlidingEntityExpiration(TimeSpan.FromMinutes(5)));
        }

        private void CacheGuild(GuildInfoResponse guild)
        {
            this._guildCache.AddOrReplace(guild, new TimeSpanEntityExpiration(TimeSpan.FromMinutes(5)));
            foreach (RoleInfoResponse role in guild.Roles)
                this.CacheRole(role);
            foreach (GuildUserInfoResponse user in guild.Users)
                this.CacheGuildUser(user);
        }

        private static string BuildWithArrayQuery<T>(string url, string queryKey, IEnumerable<T> queryValues)
        {
            if (queryValues?.Any() != true)
                return url;

            string query = string.Join('&', queryValues.Select(v => $"{queryKey}={HttpUtility.UrlEncodeUnicode(v.ToString())}"));
            return $"{url}?{query}";
        }
    }
}
