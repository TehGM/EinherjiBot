namespace TehGM.EinherjiBot.SharedAccounts
{
    public interface ISharedAccountStore
    {
        Task<SharedAccount> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<SharedAccount>> FindAsync(SharedAccountFilter filter, CancellationToken cancellationToken = default);

        Task UpdateAsync(SharedAccount account, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
