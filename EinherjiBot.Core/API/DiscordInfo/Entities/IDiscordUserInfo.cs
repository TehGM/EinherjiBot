namespace TehGM.EinherjiBot.API
{
    public interface IDiscordUserInfo : IDiscordEntityInfo
    {
        new ulong ID { get; }
        string Username { get; }
        string Discriminator { get; }
        string AvatarHash { get; }

        ulong IDiscordEntityInfo.ID => this.ID;

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
