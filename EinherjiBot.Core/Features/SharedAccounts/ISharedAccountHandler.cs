namespace TehGM.EinherjiBot.SharedAccounts
{
    /// <summary>Handles common operations on shared accounts.</summary>
    public interface ISharedAccountHandler
    {
        /// <summary>Retrieves all shared accounts available to the caller user.</summary>
        /// <param name="filter">Filters to apply when retrieving shared accounts. Is ignored during WebAPI calls.</param>
        /// <param name="skipAudit">Whether auditing access should be skipped. Auditing is always skipped for Einherji. Is ignored during WebAPI calls.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Enumerable of shared accounts. Empty if none found.</returns>
        Task<IEnumerable<SharedAccountResponse>> GetAllAsync(SharedAccountFilter filter, bool skipAudit, CancellationToken cancellationToken = default);
        /// <summary>Retrieves specific shared account.</summary>
        /// <param name="id">ID of the account.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Found shared account. Null if not found.</returns>
        Task<SharedAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>Gets image URLs for each shared account type.</summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Map of shared account type and its image URL.</returns>
        Task<IDictionary<SharedAccountType, string>> GetImagesAsync(CancellationToken cancellationToken = default);

        /// <summary>Creates a new shared account.</summary>
        /// <param name="request">Data of shared account to create.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Created shared account.</returns>
        Task<SharedAccountResponse> CreateAsync(SharedAccountRequest request, CancellationToken cancellationToken = default);

        /// <summary>Updates shared account.</summary>
        /// <param name="id">ID of account to update.</param>
        /// <param name="request">Data of new shared account state.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Updated account along with information whether any changes have been made; null if not found.</returns>
        Task<EntityUpdateResult<SharedAccountResponse>> UpdateAsync(Guid id, SharedAccountRequest request, CancellationToken cancellationToken = default);

        /// <summary>Deletes a shared account.</summary>
        /// <param name="id">ID of the account.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Awaitable task.</returns>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public static class SharedAccountsHandlerExtensions
    {
        /// <summary>Retrieves all shared accounts available to the caller user.</summary>
        /// <param name="handler">Handler to use.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Enumerable of shared accounts. Empty if none found.</returns>
        public static Task<IEnumerable<SharedAccountResponse>> GetAllAsync(this ISharedAccountHandler handler, CancellationToken cancellationToken = default)
            => handler.GetAllAsync(null, false, cancellationToken);
    }
}
