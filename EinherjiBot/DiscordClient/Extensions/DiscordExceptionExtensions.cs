using Discord;
using Discord.Net;

namespace TehGM.EinherjiBot
{
    public static class DiscordExceptionExtensions
    {
        public static bool IsMissingPermissions(this Exception exception)
            => exception != null &&
                exception is HttpException ex &&
                (ex.DiscordCode == DiscordErrorCode.MissingPermissions || ex.DiscordCode == DiscordErrorCode.InsufficientPermissions);
    }
}
