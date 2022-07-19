using Discord;

namespace TehGM.EinherjiBot.Intel
{
    public static class DiscordUserStatusExtensions
    {
        public static bool IsOnlineStatus(this UserStatus status)
            => status != UserStatus.Offline;
    }
}
