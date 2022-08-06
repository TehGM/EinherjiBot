namespace TehGM.EinherjiBot.Security
{
    public interface IAuthContext
    {
        ulong ID { get; }
        string Username { get; }
        string Discriminator { get; }
        string AvatarHash { get; }
        IEnumerable<string> BotRoles { get; }
        bool IsBanned { get; }

        /// <summary>User's guilds that bot can track.</summary>
        IEnumerable<ulong> KnownDiscordGuildIDs { get; }
        /// <summary>User's roles that bot can track.</summary>
        IEnumerable<ulong> KnownDiscordRoleIDs { get; }

        string GetUsernameWithDiscriminator()
            => $"{this.Username}#{this.Discriminator}";

        string GetAvatarURL(ushort size = 1024)
        {
            const string baseUrl = "https://cdn.discordapp.com";
            if (string.IsNullOrWhiteSpace(this.AvatarHash))
            {
                int value = int.Parse(this.Discriminator) % 5;
                return $"{baseUrl}/embed/avatars/{value}.png";
            }

            string ext = this.AvatarHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png";
            return $"{baseUrl}/avatars/{this.ID}/{this.AvatarHash}.{ext}?size={size}";
        }
    }
}
