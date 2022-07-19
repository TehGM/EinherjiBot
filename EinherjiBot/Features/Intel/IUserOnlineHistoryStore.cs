namespace TehGM.EinherjiBot.Intel
{
    public interface IUserOnlineHistoryStore
    {
        Task<UserOnlineHistory> GetAsync(ulong userID, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserOnlineHistory data, CancellationToken cancellationToken = default);
    }
}
