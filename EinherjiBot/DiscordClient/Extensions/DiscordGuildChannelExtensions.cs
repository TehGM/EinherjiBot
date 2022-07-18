using Discord;

namespace TehGM.EinherjiBot
{
    public static class DiscordGuildChannelExtensions
    {
        public static string GetURL(this IGuildChannel channel)
            => $"https://discord.com/channels/{channel.GuildId}/{channel.Id}";
    }
}
