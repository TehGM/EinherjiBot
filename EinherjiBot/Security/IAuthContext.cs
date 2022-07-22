using Discord;

namespace TehGM.EinherjiBot.Security
{
    public interface IAuthContext
    {
        ulong UserID { get; }
        IUser DiscordUser { get; }
        IGuild DiscordGuild { get; }
        IGuildUser DiscordGuildUser { get; }
        IEnumerable<string> BotRoles { get; }
        bool IsBanned { get; }

        IEnumerable<ulong> KnownDiscordRoleIDs { get; }
    }
}
