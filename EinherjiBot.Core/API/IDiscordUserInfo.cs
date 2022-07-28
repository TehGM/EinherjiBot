namespace TehGM.EinherjiBot.API
{
    public interface IDiscordUserInfo
    {
        ulong ID { get; }
        string Username { get; }
        string Discriminator { get; }
        string AvatarHash { get; }

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

        string GetUsernameWithDiscriminator()
            => $"{this.Username}#{this.Discriminator}";
    }
}
