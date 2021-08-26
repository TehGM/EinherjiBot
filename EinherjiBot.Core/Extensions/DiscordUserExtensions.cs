using DSharpPlus.Entities;

namespace TehGM.EinherjiBot
{
    public static class DiscordUserExtensions
    {
        public static string GetSafeAvatarUrl(this DiscordUser user, DSharpPlus.ImageFormat format = DSharpPlus.ImageFormat.Auto, ushort size = 2048)
            => user.GetAvatarUrl(format, size) ?? user.DefaultAvatarUrl;
    }
}
