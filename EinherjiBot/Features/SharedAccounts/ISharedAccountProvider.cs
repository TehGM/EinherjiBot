namespace TehGM.EinherjiBot.SharedAccounts
{
    public interface ISharedAccountProvider
    {
        Task<IEnumerable<SharedAccount>> GetAllAuthorizedAsync(bool forModeration, CancellationToken cancellationToken = default);
        Task<SharedAccount> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<SharedAccount>> GetAuthorizedOfTypeAsync(SharedAccountType type, bool forModeration, CancellationToken cancellationToken = default);

        Task AddOrUpdateAsync(SharedAccount account, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
