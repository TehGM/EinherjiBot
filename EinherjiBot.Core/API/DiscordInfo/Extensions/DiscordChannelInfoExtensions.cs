using Discord;

namespace TehGM.EinherjiBot.API
{
    public static class DiscordChannelInfoExtensions
    {
        public static bool IsThread(this ChannelType channelType)
            => channelType == ChannelType.NewsThread || channelType == ChannelType.PrivateThread || channelType == ChannelType.PublicThread;
        public static bool IsThreadChannel(this ChannelInfoResponse channel)
            => channel != null && channel.Type.IsThread();
    }
}
