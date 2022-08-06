namespace TehGM.EinherjiBot.SharedAccounts.API
{
    public interface ISharedAccountsService
    {
        Task<IEnumerable<SharedAccountResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<SharedAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);

        Task<SharedAccountResponse> CreateAsync(SharedAccountRequest request, CancellationToken cancellationToken = default);

        Task<SharedAccountResponse> UpdateAsync(Guid id, SharedAccountRequest request, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
