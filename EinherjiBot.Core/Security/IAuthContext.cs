namespace TehGM.EinherjiBot.Security
{
    public interface IAuthContext
    {
        ulong ID { get; }
        string Username { get; }
        string GlobalName { get; }
        string Discriminator { get; }
        string AvatarHash { get; }
        IEnumerable<string> BotRoles { get; }
        bool IsBanned { get; }

        /// <summary>User's guilds that bot can track.</summary>
        IEnumerable<ulong> RecognizedDiscordGuildIDs { get; }
        /// <summary>User's roles that bot can track.</summary>
        IEnumerable<ulong> RecognizedDiscordRoleIDs { get; }

        string GetDisplayName()
            => string.IsNullOrWhiteSpace(this.GlobalName) ? this.Username : this.GlobalName;

        string GetAvatarURL(ushort size = 1024)
            => EinherjiBot.API.DiscordUserInfoExtensions.GetAvatarURL(this.ID, this.AvatarHash, this.Discriminator, size);
    }
}
