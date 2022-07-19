namespace TehGM.EinherjiBot.Security
{
    public interface IUserContextProvider
    {
        Task<IUserContext> GetUserContextAsync(ulong userID, CancellationToken cancellationToken = default);
    }
}
