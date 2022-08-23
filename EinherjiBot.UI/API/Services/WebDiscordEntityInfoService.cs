using System.Collections.Generic;
using System.Web;
using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Caching.Services;

namespace TehGM.EinherjiBot.UI.API.Services
{
    public class WebDiscordEntityInfoService : IDiscordEntityInfoService
    {
        private readonly IApiClient _client;
        private readonly IDiscordEntityInfoCache _caches;
        private readonly ILockProvider _lock;

        public WebDiscordEntityInfoService(IApiClient client, ILockProvider<WebDiscordEntityInfoService> lockProvider, IDiscordEntityInfoCache caches)
        {
            this._client = client;
            this._caches = caches;
            this._lock = lockProvider;
        }

        public async ValueTask<UserInfoResponse> GetBotInfoAsync(CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!this._caches.TryGetBotInfo(out UserInfoResponse result))
                {
                    result = await this._client.GetJsonAsync<UserInfoResponse>("entity-info/bot", cancellationToken).ConfigureAwait(false);
                    this._caches.CacheBotInfo(result);
                }
                return result;
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
                if (this._caches.UsersCache.TryGet(userID, out UserInfoResponse result))
                    return result;

                result = await this._client.GetJsonAsync<UserInfoResponse>($"entity-info/user/{userID}", cancellationToken).ConfigureAwait(false);
                this._caches.CacheUser(userID, result);
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
                    if (!this._caches.TryGetAllGuilds(out IEnumerable<GuildInfoResponse> results))
                    {
                        results = await this._client.GetJsonAsync<IEnumerable<GuildInfoResponse>>(url, cancellationToken).ConfigureAwait(false);
                        this._caches.CacheAllGuilds(results);
                    }
                    return results;
                }
                else
                {
                    List<GuildInfoResponse> results = new List<GuildInfoResponse>(ids.Count());

                    // if some are cached, we don't need to retrieve them from server, eh?
                    IEnumerable<CachedEntity<ulong, GuildInfoResponse>> cachedGuilds = this._caches.GuildCache.Scan(g => ids.Contains(g.Key));
                    foreach (GuildInfoResponse cached in cachedGuilds.Select(g => g.Entity))
                        results.Add(cached);
                    IEnumerable<ulong> remaining = ids.Except(cachedGuilds.Select(g => g.Key));

                    if (remaining.Any())
                    {
                        url = BuildWithArrayQuery(url, "guild", remaining);
                        IEnumerable<GuildInfoResponse> response = await this._client.GetJsonAsync<IEnumerable<GuildInfoResponse>>(url, cancellationToken).ConfigureAwait(false);
                        foreach (ulong id in remaining)
                        {
                            GuildInfoResponse guild = response?.FirstOrDefault(g => g.ID == id);
                            results.Add(guild);
                            this._caches.CacheGuild(id, guild);
                        }
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
                if (this._caches.GuildUserCache.TryGet(key, out GuildUserInfoResponse result))
                    return result;

                result = await this._client.GetJsonAsync<GuildUserInfoResponse>($"entity-info/guild/{guildID}/user/{userID}", cancellationToken).ConfigureAwait(false);
                this._caches.CacheGuildUser(userID, result);
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
                if (this._caches.RolesCache.TryGet(roleID, out RoleInfoResponse result))
                    return result;

                string url = BuildWithArrayQuery($"entity-info/role/{roleID}", "guild", guildIDs);
                result = await this._client.GetJsonAsync<RoleInfoResponse>(url, cancellationToken).ConfigureAwait(false);
                this._caches.CacheRole(roleID, result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<ChannelInfoResponse> GetChannelInfoAsync(ulong channelID, IEnumerable<ulong> guildIDs, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this._caches.ChannelsCache.TryGet(channelID, out ChannelInfoResponse result))
                    return result;

                string url = BuildWithArrayQuery($"entity-info/channel/{channelID}", "guild", guildIDs);
                result = await this._client.GetJsonAsync<ChannelInfoResponse>(url, cancellationToken).ConfigureAwait(false);
                this._caches.CacheChannel(channelID, result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
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
