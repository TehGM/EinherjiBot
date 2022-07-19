namespace TehGM.EinherjiBot.Intel
{
    public interface IUserIntelProvider
    {
        Task<UserIntel> GetAsync(ulong userID, ulong? guildID, CancellationToken cancellationToken = default);
        Task UpdateHistoryAsync(UserOnlineHistory intel, CancellationToken cancellationToken = default);
    }
}
