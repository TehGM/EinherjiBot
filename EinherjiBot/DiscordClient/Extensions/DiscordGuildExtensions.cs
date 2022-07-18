using Discord;

namespace TehGM.EinherjiBot
{
    public static class DiscordGuildExtensions
    {
        public static Task<IGuildUser> GetGuildUserAsync(this IGuild guild, ulong id, CancellationToken cancellationToken = default)
            => guild.GetUserAsync(id, CacheMode.AllowDownload, cancellationToken.ToRequestOptions());
        public static Task<IGuildUser> GetGuildUserAsync(this IGuildChannel channel, ulong id)
            => GetGuildUserAsync(channel.Guild, id);
        public static Task<IGuildUser> GetGuildUserAsync(this IGuildChannel channel, IUser user)
            => GetGuildUserAsync(channel, user.Id);
        public static Task<IGuildUser> GetGuildUserAsync(this IGuild guild, IUser user)
            => GetGuildUserAsync(guild, user.Id);

        public static Task<IGuildChannel> GetChannelAsync(this IGuild guild, ulong id, CancellationToken cancellationToken = default)
            => guild.GetChannelAsync(id, CacheMode.AllowDownload, cancellationToken.ToRequestOptions());
    }
}
