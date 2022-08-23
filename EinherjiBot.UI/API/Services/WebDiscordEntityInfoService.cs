﻿using System.Web;
using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Caching.Services;

namespace TehGM.EinherjiBot.UI.API.Services
{
    public class WebDiscordEntityInfoService : IDiscordEntityInfoService
    {
        public record struct GuildUserKey(ulong UserID, ulong GuildID);

        private readonly IApiClient _client;
        private readonly IEntityCache<ulong, UserInfoResponse> _usersCache;
        private readonly IEntityCache<ulong, RoleInfoResponse> _rolesCache;
        private readonly IEntityCache<ulong, ChannelInfoResponse> _channelsCache;
        private readonly IEntityCache<GuildUserKey, GuildUserInfoResponse> _guildUserCache;
        private readonly IEntityCache<ulong, GuildInfoResponse> _guildCache;
        private readonly ILockProvider _lock;

        private ulong? _botID = null;
        private IEnumerable<GuildInfoResponse> _cachedAllGuilds;
        private DateTime _cachedAllGuildsTimestamp;

        public WebDiscordEntityInfoService(IApiClient client, ILockProvider<WebDiscordEntityInfoService> lockProvider, 
            IEntityCache<ulong, UserInfoResponse> usersCache, IEntityCache<ulong, RoleInfoResponse> rolesCache, IEntityCache<ulong, ChannelInfoResponse> channelsCache,
            IEntityCache<GuildUserKey, GuildUserInfoResponse> guildUserCache, IEntityCache<ulong, GuildInfoResponse> guildCache)
        {
            this._client = client;
            this._usersCache = usersCache;
            this._rolesCache = rolesCache;
            this._channelsCache = channelsCache;
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
                    this.CacheUser(result.ID, result);
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
                this.CacheUser(userID, result);
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
                    if (this._cachedAllGuilds != null && this._cachedAllGuildsTimestamp > DateTime.UtcNow - cacheExpiration)
                        return this._cachedAllGuilds;
                    else
                    {
                        IEnumerable<GuildInfoResponse> response = await this._client.GetJsonAsync<IEnumerable<GuildInfoResponse>>(url, cancellationToken).ConfigureAwait(false);
                        this._cachedAllGuilds = response;
                        this._cachedAllGuildsTimestamp = DateTime.UtcNow;
                        foreach (GuildInfoResponse guild in response)
                            this.CacheGuild(guild.ID, guild);
                        return response;
                    }
                }
                else
                {
                    List<GuildInfoResponse> results = new List<GuildInfoResponse>(ids.Count());

                    // if some are cached, we don't need to retrieve them from server, eh?
                    IEnumerable<CachedEntity<ulong, GuildInfoResponse>> cachedGuilds = this._guildCache.Scan(g => ids.Contains(g.Key));
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
                            this.CacheGuild(id, guild);
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
                if (this._guildUserCache.TryGet(key, out GuildUserInfoResponse result))
                    return result;

                result = await this._client.GetJsonAsync<GuildUserInfoResponse>($"entity-info/guild/{guildID}/user/{userID}", cancellationToken).ConfigureAwait(false);
                this.CacheGuildUser(userID, result);
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
                this.CacheRole(roleID, result);
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
                if (this._channelsCache.TryGet(channelID, out ChannelInfoResponse result))
                    return result;

                string url = BuildWithArrayQuery($"entity-info/channel/{channelID}", "guild", guildIDs);
                result = await this._client.GetJsonAsync<ChannelInfoResponse>(url, cancellationToken).ConfigureAwait(false);
                this.CacheChannel(channelID, result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        private void CacheGuildUser(ulong id, GuildUserInfoResponse user)
        {
            // guild user caching needs special treatment, as same user might be in different guilds
            GuildUserKey key = new GuildUserKey(id, user.GuildID);
            this._guildUserCache.AddOrReplace(key, user, new SlidingEntityExpiration(TimeSpan.FromMinutes(5)));

            this.CacheUser(id, user);
        }

        private void CacheUser(ulong id, UserInfoResponse user)
        {
            TimeSpan expiration = TimeSpan.FromMinutes(5);
            if (this._botID != null && id == this._botID)
                expiration = TimeSpan.FromMinutes(50);
            this._usersCache.AddOrReplace(id, user, new SlidingEntityExpiration(expiration));
        }

        private void CacheRole(ulong id, RoleInfoResponse role)
        {
            this._rolesCache.AddOrReplace(id, role, new SlidingEntityExpiration(TimeSpan.FromMinutes(5)));
        }

        private void CacheGuild(ulong id, GuildInfoResponse guild)
        {
            this._guildCache.AddOrReplace(id, guild, new TimeSpanEntityExpiration(TimeSpan.FromMinutes(5)));
            if (guild == null)
                return;
            foreach (RoleInfoResponse role in guild.Roles)
                this.CacheRole(role.ID, role);
            foreach (GuildUserInfoResponse user in guild.Users)
                this.CacheGuildUser(user.ID, user);
            foreach (ChannelInfoResponse channel in guild.Channels)
                this.CacheChannel(channel.ID, channel);
        }

        private void CacheChannel(ulong id, ChannelInfoResponse channel)
        {
            this._channelsCache.AddOrReplace(id, channel, new SlidingEntityExpiration(TimeSpan.FromMinutes(5)));
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
