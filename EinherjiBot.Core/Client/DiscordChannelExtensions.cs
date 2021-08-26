using DSharpPlus;
using DSharpPlus.Entities;

namespace TehGM.EinherjiBot
{
    public static class DiscordChannelExtensions
    {
        public static bool IsText(this DiscordChannel channel)
            => channel.Type == ChannelType.Text || channel.Type == ChannelType.Private || channel.Type == ChannelType.Group || channel.Type == ChannelType.News
            // threads
            || (int)channel.Type == 10 || (int)channel.Type == 11 || (int)channel.Type == 12;

        public static bool IsGuildText(this DiscordChannel channel)
            => !channel.IsPrivate && IsText(channel);
    }
}
