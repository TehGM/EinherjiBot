namespace TehGM.EinherjiBot
{
    public interface IUserDataStore
    {
        Task<UserData> GetAsync(ulong userID, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserData data, CancellationToken cancellationToken = default);
    }
}
