using Discord;
using System.Diagnostics;

namespace TehGM.EinherjiBot.Security
{
    [DebuggerDisplay("{ToString(),nq} ({ID,nq})")]
    public class DiscordSocketAuthContext : IDiscordAuthContext, IAuthContext, IEquatable<DiscordSocketAuthContext>, IEquatable<IAuthContext>
    {
        public static DiscordSocketAuthContext None => new DiscordSocketAuthContext();

        string IAuthContext.Username => this.DiscordUser?.Username;
        string IAuthContext.Discriminator => this.DiscordUser?.Discriminator;

        public IUser DiscordUser { get; }
        public IGuild DiscordGuild { get; }
        public IGuildUser DiscordGuildUser { get; }
        public IEnumerable<ulong> KnownDiscordRoleIDs { get; }

        public ulong ID => this.DiscordUser?.Id ?? default;
        public IEnumerable<string> BotRoles => this._data?.Roles;
        public bool IsBanned => this._data?.IsBanned ?? false;

        private readonly UserSecurityData _data;

        public DiscordSocketAuthContext(IUser discordUser, IGuild discordGuild, IGuildUser discordGuildUser, IEnumerable<ulong> knownRoleIDs, UserSecurityData securityData)
        {
            this.DiscordUser = discordUser ?? throw new ArgumentNullException(nameof(discordUser));
            this.DiscordGuild = discordGuild;
            this.DiscordGuildUser = discordGuildUser;
            this.KnownDiscordRoleIDs = new HashSet<ulong>(knownRoleIDs ?? Enumerable.Empty<ulong>());
            this._data = securityData ?? throw new ArgumentNullException(nameof(securityData));
        }

        private DiscordSocketAuthContext() { }

        string IAuthContext.GetAvatarURL(ushort size)
            => this.DiscordUser.GetSafeAvatarUrl(ImageFormat.Auto, size);

        public override string ToString()
            => this.DiscordUser?.GetUsernameWithDiscriminator();

        public override bool Equals(object obj)
            => this.Equals(obj as IAuthContext);
        public bool Equals(DiscordSocketAuthContext other)
            => this.Equals(other as IAuthContext);
        public bool Equals(IAuthContext other)
            => other is not null && this.ID == other.ID;
        public override int GetHashCode()
            => HashCode.Combine(this.ID);
        public static bool operator ==(DiscordSocketAuthContext left, DiscordSocketAuthContext right)
            => EqualityComparer<DiscordSocketAuthContext>.Default.Equals(left, right);
        public static bool operator !=(DiscordSocketAuthContext left, DiscordSocketAuthContext right)
            => !(left == right);
    }
}
