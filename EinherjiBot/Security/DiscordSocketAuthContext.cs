using Discord;

namespace TehGM.EinherjiBot.Security
{
    public class DiscordSocketAuthContext : IAuthContext
    {
        public ulong UserID => this.DiscordUser.Id;
        public IUser DiscordUser { get; }
        public IGuild DiscordGuild { get; }
        public IGuildUser DiscordGuildUser { get; }
        public IEnumerable<string> BotRoles => this._data.Roles;
        public bool IsBanned => this._data.IsBanned;
        public IEnumerable<ulong> KnownDiscordRoleIDs { get; }

        private readonly UserSecurityData _data;

        public DiscordSocketAuthContext(IUser discordUser, IGuild discordGuild, IGuildUser discordGuildUser, IEnumerable<ulong> knownRoleIDs, UserSecurityData securityData)
        {
            this.DiscordUser = discordUser ?? throw new ArgumentNullException(nameof(discordUser));
            this.DiscordGuild = discordGuild;
            this.DiscordGuildUser = discordGuildUser;
            this.KnownDiscordRoleIDs = new HashSet<ulong>(knownRoleIDs ?? Enumerable.Empty<ulong>());
            this._data = securityData ?? throw new ArgumentNullException(nameof(securityData));
        }
    }
}
