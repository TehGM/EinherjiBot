namespace TehGM.EinherjiBot.SharedAccounts
{
    public static class SharedAccountExtensions
    {
        /// <summary>Checks if provided request would make any changes to given shared account.</summary>
        /// <remarks>Order of IDs in ACLs is ignored.</remarks>
        /// <param name="account">Existing account.</param>
        /// <param name="request">Data of new shared account state.</param>
        /// <returns>Whether <paramref name="request"/> would change <paramref name="account"/> in any way.</returns>
        public static bool HasChanges(this ISharedAccount account, SharedAccountRequest request)
        {
            return account.Login != request.Login
                || account.Password != request.Password
                || account.AccountType != request.AccountType
                || !account.AuthorizedUserIDs.Same(request.AuthorizedUserIDs, true)
                || !account.AuthorizedRoleIDs.Same(request.AuthorizedRoleIDs, true)
                || !account.ModUserIDs.Same(request.ModUserIDs, true);
        }
    }
}
