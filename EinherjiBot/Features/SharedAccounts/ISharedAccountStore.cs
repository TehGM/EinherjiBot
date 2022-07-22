namespace TehGM.EinherjiBot.SharedAccounts
{
    public interface ISharedAccountStore
    {
        Task<SharedAccount> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<SharedAccount>> FindAsync(SharedAccountType? type, ulong? userID, IEnumerable<ulong> roleIDs, bool forModeration, CancellationToken cancellationToken = default);

        Task UpdateAsync(SharedAccount account, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
