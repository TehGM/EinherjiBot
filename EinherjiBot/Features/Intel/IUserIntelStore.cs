namespace TehGM.EinherjiBot.Intel
{
    public interface IUserIntelStore
    {
        Task<UserIntel> GetAsync(ulong userID, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserIntel data, CancellationToken cancellationToken = default);
    }
}
