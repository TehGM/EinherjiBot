namespace TehGM.EinherjiBot.API
{
    public interface IDiscordGuildInfo : IDiscordEntityInfo
    {
        new ulong ID { get; }
        new string Name { get; }
        string IconHash { get; }

        ulong IDiscordEntityInfo.ID => this.ID;
        string IDiscordEntityInfo.Name => this.Name;
    }
}
