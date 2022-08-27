using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Auditing.SharedAccounts;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.SharedAccounts.Policies;

namespace TehGM.EinherjiBot.SharedAccounts.Services
{
    public class ServerSharedAccountHandler : ISharedAccountHandler
    {
        private readonly ISharedAccountProvider _provider;
        private readonly ISharedAccountImageProvider _imageProvider;
        private readonly IAuthContext _user;
        private readonly IBotAuthorizationService _auth;
        private readonly SharedAccountOptions _options;
        private readonly IAuditStore<SharedAccountAuditEntry> _audit;

        public ServerSharedAccountHandler(ISharedAccountProvider provider, ISharedAccountImageProvider imageProvider, IOptionsSnapshot<SharedAccountOptions> options,
            IAuthContext user, IBotAuthorizationService auth, IAuditStore<SharedAccountAuditEntry> audit)
        {
            this._provider = provider;
            this._imageProvider = imageProvider;
            this._user = user;
            this._auth = auth;
            this._options = options.Value;
            this._audit = audit;
        }

        public async Task<IEnumerable<SharedAccountResponse>> GetAllAsync(SharedAccountFilter filter, bool skipAudit, CancellationToken cancellationToken = default)
        {
            filter ??= new SharedAccountFilter();
            filter.FilterUserAccess(this._user);

            IEnumerable<ISharedAccount> accounts = await this._provider.FindAsync(filter, cancellationToken).ConfigureAwait(false);
            List<SharedAccountResponse> results = new List<SharedAccountResponse>(accounts.Count());
            DateTime auditTimestamp = DateTime.UtcNow;
            foreach (ISharedAccount account in accounts)
            {
                BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(account, typeof(CanAccessSharedAccount), cancellationToken).ConfigureAwait(false);
                if (authorization.Succeeded)
                    results.Add(await this.CreateResponseAsync(account, cancellationToken).ConfigureAwait(false));
                if (!skipAudit)
                    await this.AuditAsync(SharedAccountAuditEntry.Retrieved(this._user.ID, account.ID, auditTimestamp), cancellationToken).ConfigureAwait(false);
            }
            return results.ToArray();
        }

        public async Task<SharedAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ISharedAccount account = await this._provider.GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (account == null)
                return null;

            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(account, typeof(CanAccessSharedAccount), cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException($"No permissions to access shared account {new Base64Guid(id)}.");

            await this.AuditAsync(SharedAccountAuditEntry.Retrieved(this._user.ID, account.ID, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
            return await this.CreateResponseAsync(account, cancellationToken).ConfigureAwait(false);
        }

        public async Task<SharedAccountResponse> CreateAsync(SharedAccountRequest request, CancellationToken cancellationToken = default)
        {
            request.ThrowValidateForCreation();

            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(new[] { typeof(CanCreateSharedAccount) }, cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException($"No permissions to create shared accounts.");

            SharedAccount account = new SharedAccount(request.AccountType);
            this.ApplyChanges(account, request);

            await this._provider.AddOrUpdateAsync(account, cancellationToken).ConfigureAwait(false);
            await this.AuditAsync(SharedAccountAuditEntry.Created(this._user.ID, account.ID, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
            return await this.CreateResponseAsync(account, cancellationToken).ConfigureAwait(false);
        }

        public async Task<EntityUpdateResult<SharedAccountResponse>> UpdateAsync(Guid id, SharedAccountRequest request, CancellationToken cancellationToken = default)
        {
            SharedAccount account = await this._provider.GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (account == null)
                return null;

            request.ThrowValidateForUpdate(account);

            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync<ISharedAccount>(account, new[] { typeof(CanAccessSharedAccount), typeof(CanEditSharedAccount) }, cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException($"No permissions to edit shared account {new Base64Guid(account.ID)}.");

            if (account.HasChanges(request))
            {
                this.ApplyChanges(account, request);
                await this._provider.AddOrUpdateAsync(account, cancellationToken).ConfigureAwait(false);
                await this.AuditAsync(SharedAccountAuditEntry.Updated(this._user.ID, account.ID, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                SharedAccountResponse response = await this.CreateResponseAsync(account, cancellationToken).ConfigureAwait(false);
                return IEntityUpdateResult.Saved(response);
            }
            else
            {
                SharedAccountResponse response = await this.CreateResponseAsync(account, cancellationToken).ConfigureAwait(false);
                return IEntityUpdateResult.NoChanges(response);
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ISharedAccount account = await this._provider.GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (account == null)
                return; 

            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(account, typeof(CanDeleteSharedAccount) , cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException($"No permissions to delete shared account {id}.");

            await this._provider.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await this.AuditAsync(SharedAccountAuditEntry.Deleted(this._user.ID, account.ID, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
        }

        public Task<IDictionary<SharedAccountType, string>> GetImagesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(this._options.ImageURLs);

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
            // authorization data is rather sensitive, so only return it if user is a mod
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

        private Task AuditAsync(SharedAccountAuditEntry entry, CancellationToken cancellationToken = default)
        {
            if (this._user.IsEinherji())
                return Task.CompletedTask;

            return this._audit.AddAuditAsync(entry, cancellationToken);
        }
    }
}
