namespace TehGM.EinherjiBot.API
{
    public interface IDiscordUserInfo : IDiscordEntityInfo
    {
        new ulong ID { get; }
        string Username { get; }
        string Discriminator { get; }
        string AvatarHash { get; }
        bool IsBot { get; }

        ulong IDiscordEntityInfo.ID => this.ID;
    }
}
