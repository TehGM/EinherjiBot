using Discord;
using MudBlazor;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.UI.Components.EntityInfo
{
    public static class ChannelIcon
    {
        public static string GetForType(ChannelType type)
        {
            if (type == ChannelType.Text)
                return Icons.Material.Filled.Numbers;
            if (type.IsThread())
                return Icons.Material.Filled.SubdirectoryArrowRight;
            if (type == ChannelType.Category)
                return Icons.Material.Filled.AutoAwesomeMotion;
            if (type == ChannelType.Voice)
                return Icons.Material.Filled.VolumeUp;
            return null;
        }

        public static string GetForChannel(ChannelInfoResponse channel)
            => GetForType(channel.Type);
    }
}
