﻿using Discord;
using System.Diagnostics;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.Security
{
    [DebuggerDisplay("{ToString(),nq} ({ID,nq})")]
    public class DiscordSocketAuthContext : IDiscordAuthContext, IAuthContext, IDiscordUserInfo, IEquatable<DiscordSocketAuthContext>, IEquatable<IAuthContext>
    {
        public static DiscordSocketAuthContext None => new DiscordSocketAuthContext();

        public IUser DiscordUser { get; }
        public IGuild DiscordGuild { get; }
        public IGuildUser DiscordGuildUser { get; }
        public IEnumerable<ulong> RecognizedDiscordGuildIDs { get; }
        public IEnumerable<ulong> RecognizedDiscordRoleIDs { get; }

        public ulong ID => this.DiscordUser?.Id ?? default;
        public IEnumerable<string> BotRoles => this._data?.Roles;
        public bool IsBanned => this._data?.IsBanned ?? false;

        string IAuthContext.Username => this.DiscordUser?.Username;
        string IAuthContext.Discriminator => this.DiscordUser?.Discriminator;
        string IAuthContext.AvatarHash => this.DiscordUser?.AvatarId;
        string IDiscordUserInfo.Username => this.DiscordUser?.Username;
        string IDiscordUserInfo.Discriminator => this.DiscordUser?.Discriminator;
        string IDiscordUserInfo.AvatarHash => this.DiscordUser?.AvatarId;
        bool IDiscordUserInfo.IsBot => this.DiscordUser?.IsBot ?? false;
        string IDiscordEntityInfo.Name => this.DiscordUser?.Username;

        private readonly UserSecurityData _data;

        public DiscordSocketAuthContext(IUser discordUser, IGuild discordGuild, IGuildUser discordGuildUser, IEnumerable<ulong> recognizedGuildIDs, IEnumerable<ulong> recognizedRoleIDs, UserSecurityData securityData)
        {
            this.DiscordUser = discordUser ?? throw new ArgumentNullException(nameof(discordUser));
            this.DiscordGuild = discordGuild;
            this.DiscordGuildUser = discordGuildUser;
            this.RecognizedDiscordGuildIDs = new HashSet<ulong>(recognizedGuildIDs ?? Enumerable.Empty<ulong>());
            this.RecognizedDiscordRoleIDs = new HashSet<ulong>(recognizedRoleIDs ?? Enumerable.Empty<ulong>());
            this._data = securityData ?? throw new ArgumentNullException(nameof(securityData));
        }

        private DiscordSocketAuthContext() { }

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

        public ulong GetCacheKey()
            => this.ID;

        public static bool operator ==(DiscordSocketAuthContext left, DiscordSocketAuthContext right)
            => EqualityComparer<DiscordSocketAuthContext>.Default.Equals(left, right);
        public static bool operator !=(DiscordSocketAuthContext left, DiscordSocketAuthContext right)
            => !(left == right);
    }
}
