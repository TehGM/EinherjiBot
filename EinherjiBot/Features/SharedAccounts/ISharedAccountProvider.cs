namespace TehGM.EinherjiBot.SharedAccounts
{
    public interface ISharedAccountProvider
    {
        Task<SharedAccount> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<SharedAccount>> GetOfTypeAsync(SharedAccountType type, bool forModeration, CancellationToken cancellationToken = default);

        Task UpdateAsync(SharedAccount account, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
