using Discord;

namespace TehGM.EinherjiBot.Security
{
    public interface IDiscordAuthContext : IAuthContext
    {
        IUser DiscordUser { get; }
        IGuild DiscordGuild { get; }
        IGuildUser DiscordGuildUser { get; }
        bool IsBanned { get; }

        IEnumerable<ulong> KnownDiscordRoleIDs { get; }
    }
}
