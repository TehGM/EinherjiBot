namespace TehGM.EinherjiBot.SharedAccounts
{
    public interface ISharedAccountProvider
    {
        Task<SharedAccount> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<SharedAccount>> FindAsync(SharedAccountFilter filter, CancellationToken cancellationToken = default);

        Task AddOrUpdateAsync(SharedAccount account, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
