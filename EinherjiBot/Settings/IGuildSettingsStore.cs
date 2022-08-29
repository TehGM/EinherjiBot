namespace TehGM.EinherjiBot.Settings
{
    public interface IGuildSettingsStore
    {
        Task<GuildSettings> GetAsync(ulong guildID, CancellationToken cancellationToken = default);

        Task UpdateAsync(GuildSettings setting, CancellationToken cancellationToken = default);
    }
}
