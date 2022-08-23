using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.UI.API
{
    public interface IDiscordEntityInfoCache
    {
        IEntityCache<ulong, UserInfoResponse> UsersCache { get; }
        IEntityCache<ulong, RoleInfoResponse> RolesCache { get; }
        IEntityCache<ulong, ChannelInfoResponse> ChannelsCache { get; }
        IEntityCache<GuildUserKey, GuildUserInfoResponse> GuildUserCache { get; }
        IEntityCache<ulong, GuildInfoResponse> GuildCache { get; }

        bool TryGetBotInfo(out UserInfoResponse result);
        bool TryGetAllGuilds(out IEnumerable<GuildInfoResponse> results);

        void CacheBotInfo(UserInfoResponse user);
        void CacheAllGuilds(IEnumerable<GuildInfoResponse> guilds);
        void CacheGuildUser(ulong id, GuildUserInfoResponse user);
        void CacheUser(ulong id, UserInfoResponse user);
        void CacheRole(ulong id, RoleInfoResponse role);
        void CacheGuild(ulong id, GuildInfoResponse guild);
        void CacheChannel(ulong id, ChannelInfoResponse channel);

        void ClearAll();
    }
}
