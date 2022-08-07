using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.SharedAccounts.Policies;

namespace TehGM.EinherjiBot.SharedAccounts.API.Services
{
    public class ApiSharedAccountsService : ISharedAccountsService
    {
        private readonly ISharedAccountProvider _provider;
        private readonly ISharedAccountImageProvider _imageProvider;
        private readonly IAuthContext _user;
        private readonly IBotAuthorizationService _auth;
        private readonly SharedAccountOptions _options;

        public ApiSharedAccountsService(ISharedAccountProvider provider, ISharedAccountImageProvider imageProvider, IOptionsSnapshot<SharedAccountOptions> options,
            IAuthContext user, IBotAuthorizationService auth)
        {
            this._provider = provider;
            this._imageProvider = imageProvider;
            this._user = user;
            this._auth = auth;
            this._options = options.Value;
        }

        public async Task<IEnumerable<SharedAccountResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<ISharedAccount> accounts = await this._provider.GetAllAuthorizedAsync(false, cancellationToken).ConfigureAwait(false);
            List<SharedAccountResponse> results = new List<SharedAccountResponse>(accounts.Count());
            foreach (ISharedAccount account in accounts)
                results.Add(await this.CreateResponseAsync(account, cancellationToken).ConfigureAwait(false));
            return results.ToArray();
        }

        public async Task<SharedAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ISharedAccount account = await this._provider.GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (account == null)
                return null;
            return await this.CreateResponseAsync(account, cancellationToken).ConfigureAwait(false);
        }

        public async Task<SharedAccountResponse> CreateAsync(SharedAccountRequest request, CancellationToken cancellationToken = default)
        {
            SharedAccount account = new SharedAccount(request.AccountType);
            this.ApplyChanges(account, request);

            await this._provider.AddOrUpdateAsync(account, cancellationToken).ConfigureAwait(false);
            return await this.CreateResponseAsync(account, cancellationToken).ConfigureAwait(false);
        }

        public async Task<SharedAccountResponse> UpdateAsync(Guid id, SharedAccountRequest request, CancellationToken cancellationToken = default)
        {
            SharedAccount account = await this._provider.GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (account == null)
                return null;

            this.ApplyChanges(account, request);
            await this._provider.AddOrUpdateAsync(account, cancellationToken).ConfigureAwait(false);
            return await this.CreateResponseAsync(account, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ISharedAccount account = await this._provider.GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (account == null)
                return;

            await this._provider.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        }

        private void ApplyChanges(SharedAccount account, SharedAccountRequest request)
        {
            account.Login = request.Login;
            account.Password = request.Password;
            account.AuthorizedUserIDs = request.AuthorizedUserIDs;
            account.AuthorizedRoleIDs = request.AuthorizedRoleIDs;
            account.ModUserIDs = request.ModUserIDs;
            account.ModifiedByID = this._user.ID;
            account.ModifiedTimestamp = DateTime.UtcNow;
        }

        private async Task<SharedAccountResponse> CreateResponseAsync(ISharedAccount account, CancellationToken cancellationToken)
        {
            BotAuthorizationResult modifyAuthorization = await this._auth.AuthorizeAsync(account, typeof(CanEditSharedAccount), cancellationToken).ConfigureAwait(false);
            IEnumerable<ulong> authorizedUserIDs = modifyAuthorization.Succeeded ? account.AuthorizedUserIDs : null;
            IEnumerable<ulong> authorizedRoleIDs = modifyAuthorization.Succeeded ? account.AuthorizedRoleIDs : null;
            IEnumerable<ulong> modUserIDs = modifyAuthorization.Succeeded ? account.ModUserIDs : null;

            return new SharedAccountResponse(account.ID, account.AccountType, account.Login, account.Password)
            {
                ImageURL = await this._imageProvider.GetAccountImageUrlAsync(account.AccountType, cancellationToken).ConfigureAwait(false),
                AuthorizedUserIDs = authorizedUserIDs,
                AuthorizedRoleIDs = authorizedRoleIDs,
                ModUserIDs = modUserIDs,
                ModifiedByID = account.ModifiedByID,
                ModifiedTimestamp = account.ModifiedTimestamp
            };
        }

        public Task<IDictionary<SharedAccountType, string>> GetImagesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(this._options.ImageURLs);
    }
}
