namespace TehGM.EinherjiBot.API
{
    public static class DiscordUserInfoExtensions
    {
        public static string GetAvatarURL(this IDiscordUserInfo user, ushort size = 1024)
        {
            const string baseUrl = "https://cdn.discordapp.com";
            if (string.IsNullOrWhiteSpace(user.AvatarHash))
            {
                int value = int.Parse(user.Discriminator) % 5;
                return $"{baseUrl}/embed/avatars/{value}.png";
            }

            string ext = user.AvatarHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png";
            return $"{baseUrl}/avatars/{user.ID}/{user.AvatarHash}.{ext}?size={size}";
        }

        public static string GetUsernameWithDiscriminator(this IDiscordUserInfo user)
            => $"{user.Username}#{user.Discriminator}";
    }
}
