namespace TehGM.EinherjiBot.Settings
{
    public interface IGuildSettingsProvider
    {
        Task<GuildSettings> GetAsync(ulong guildID, CancellationToken cancellationToken = default);

        Task AddOrUpdateAsync(GuildSettings setting, CancellationToken cancellationToken = default);
    }
}
