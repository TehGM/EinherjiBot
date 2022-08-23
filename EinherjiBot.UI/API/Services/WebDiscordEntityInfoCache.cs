using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.UI.API.Services
{
    // because auth is hard, currently using entity info provider from auth services results in circular dependencies
    // this prevents caching on logout. This service is meant to address that until a better approach is found
    public class WebDiscordEntityInfoCache : IDiscordEntityInfoCache, IDisposable
    {
        public IEntityCache<ulong, UserInfoResponse> UsersCache { get; }
        public IEntityCache<ulong, RoleInfoResponse> RolesCache { get; }
        public IEntityCache<ulong, ChannelInfoResponse> ChannelsCache { get; }
        public IEntityCache<GuildUserKey, GuildUserInfoResponse> GuildUserCache { get; }
        public IEntityCache<ulong, GuildInfoResponse> GuildCache { get; }

        private ulong? _botID = null;
        private IEnumerable<GuildInfoResponse> _cachedAllGuilds;
        private DateTime _cachedAllGuildsTimestamp;

        public WebDiscordEntityInfoCache(IEntityCache<ulong, UserInfoResponse> usersCache, IEntityCache<ulong, RoleInfoResponse> rolesCache, IEntityCache<ulong, ChannelInfoResponse> channelsCache,
            IEntityCache<GuildUserKey, GuildUserInfoResponse> guildUserCache, IEntityCache<ulong, GuildInfoResponse> guildCache)
        {
            this.UsersCache = usersCache;
            this.RolesCache = rolesCache;
            this.ChannelsCache = channelsCache;
            this.GuildUserCache = guildUserCache;
            this.GuildCache = guildCache;
        }

        public bool TryGetBotInfo(out UserInfoResponse result)
        {
            result = null;
            if (this._botID == null)
                return false;
            return this.UsersCache.TryGet(this._botID.Value, out result);
        }

        public bool TryGetAllGuilds(out IEnumerable<GuildInfoResponse> results)
        {
            results = null;
            if (this._cachedAllGuilds == null)
                return false;
            if (this._cachedAllGuildsTimestamp + TimeSpan.FromMinutes(5) < DateTime.UtcNow)
                return false;
            results = this._cachedAllGuilds;
            return true;
        }

        public void CacheBotInfo(UserInfoResponse user)
        {
            this._botID = user?.ID;

            if (user == null)
                return;
            this.CacheUser(user.ID, user);
        }

        public void CacheAllGuilds(IEnumerable<GuildInfoResponse> guilds)
        {
            this._cachedAllGuilds = guilds;
            this._cachedAllGuildsTimestamp = DateTime.UtcNow;

            if (guilds == null)
                return;
            foreach (GuildInfoResponse guild in guilds)
                this.CacheGuild(guild.ID, guild);
        }

        public void CacheGuildUser(ulong id, GuildUserInfoResponse user)
        {
            // guild user caching needs special treatment, as same user might be in different guilds
            GuildUserKey key = new GuildUserKey(id, user.GuildID);
            this.GuildUserCache.AddOrReplace(key, user, new SlidingEntityExpiration(TimeSpan.FromMinutes(5)));

            this.CacheUser(id, user);
        }

        public void CacheUser(ulong id, UserInfoResponse user)
        {
            TimeSpan expiration = TimeSpan.FromMinutes(5);
            if (this._botID != null && id == this._botID)
                expiration = TimeSpan.FromMinutes(30);
            this.UsersCache.AddOrReplace(id, user, new SlidingEntityExpiration(expiration));
        }

        public void CacheRole(ulong id, RoleInfoResponse role)
        {
            this.RolesCache.AddOrReplace(id, role, new SlidingEntityExpiration(TimeSpan.FromMinutes(5)));
        }

        public void CacheGuild(ulong id, GuildInfoResponse guild)
        {
            this.GuildCache.AddOrReplace(id, guild, new TimeSpanEntityExpiration(TimeSpan.FromMinutes(5)));
            if (guild == null)
                return;
            foreach (RoleInfoResponse role in guild.Roles)
                this.CacheRole(role.ID, role);
            foreach (GuildUserInfoResponse user in guild.Users)
                this.CacheGuildUser(user.ID, user);
            foreach (ChannelInfoResponse channel in guild.Channels)
                this.CacheChannel(channel.ID, channel);
        }

        public void CacheChannel(ulong id, ChannelInfoResponse channel)
        {
            this.ChannelsCache.AddOrReplace(id, channel, new SlidingEntityExpiration(TimeSpan.FromMinutes(5)));
        }

        public void ClearAll()
        {
            this._botID = null;
            this._cachedAllGuilds = null;
            this._cachedAllGuildsTimestamp = default;
            this.ChannelsCache.Clear();
            this.UsersCache.Clear();
            this.RolesCache.Clear();
            this.GuildUserCache.Clear();
            this.GuildCache.Clear();
        }

        public void Dispose()
            => this.ClearAll();
    }
}
