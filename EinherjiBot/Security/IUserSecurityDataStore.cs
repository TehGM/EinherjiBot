namespace TehGM.EinherjiBot.Security
{
    public interface IUserSecurityDataStore
    {
        Task<UserSecurityData> GetAsync(ulong userID, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserSecurityData data, CancellationToken cancellationToken = default);
    }
}
