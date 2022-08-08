namespace TehGM.EinherjiBot.API
{
    public interface IDiscordEntityInfo : ICacheableEntity<ulong>
    {
        ulong ID { get; }
        string Name { get; }
    }
}
