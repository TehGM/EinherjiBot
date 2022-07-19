using Discord;

namespace TehGM.EinherjiBot
{
    public static class DiscordClientExtensions
    {
        public static Task<IUser> GetUserAsync(this IDiscordClient client, ulong id, CancellationToken cancellationToken = default)
            => client.GetUserAsync(id, CacheMode.AllowDownload, cancellationToken.ToRequestOptions());
        public static Task<IGuild> GetGuildAsync(this IDiscordClient client, ulong id, CancellationToken cancellationToken = default)
            => client.GetGuildAsync(id, CacheMode.AllowDownload, cancellationToken.ToRequestOptions());
    }
}
