namespace TehGM.EinherjiBot.API
{
    public static class DiscordUserInfoExtensions
    {
        public static string GetAvatarURL(this IDiscordUserInfo user, ushort size = 1024)
            => GetAvatarURL(user.ID, user.AvatarHash, user.Discriminator, size);

        /// <summary>Gets user's display name if set, or username otherwise.</summary>
        public static string GetDisplayName(this IDiscordUserInfo user)
            => string.IsNullOrWhiteSpace(user.GlobalName) ? user.Username : user.GlobalName;

        public static string GetAvatarURL(ulong userID, string avatarHash, string discriminator, ushort size = 1024)
        {
            const string baseUrl = "https://cdn.discordapp.com";
            if (string.IsNullOrWhiteSpace(avatarHash))
            {
                // users migrated to new username system have discriminator of 0
                ulong value = ushort.TryParse(discriminator, out ushort discriminatorValue) && discriminatorValue != 0
                    ? discriminatorValue % 5u
                    : (userID >> 22) % 6;
                return $"{baseUrl}/embed/avatars/{value}.png";
            }

            string ext = avatarHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png";
            return $"{baseUrl}/avatars/{userID}/{avatarHash}.{ext}?size={size}";
        }
    }
}
