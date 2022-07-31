using TehGM.EinherjiBot.Security.Authorization;

namespace TehGM.EinherjiBot.SharedAccounts.Policies
{
    public class CanDeleteSharedAccount : IDiscordAuthorizationPolicy<SharedAccount>
    {
        private readonly IDiscordAuthContext _auth;

        public CanDeleteSharedAccount(IDiscordAuthContext auth)
        {
            this._auth = auth;
        }

        public Task<DiscordAuthorizationResult> EvaluateAsync(SharedAccount resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.IsAdmin() || resource.ModUserIDs.Contains(this._auth.ID))
                return Task.FromResult(DiscordAuthorizationResult.Success);
            return Task.FromResult(DiscordAuthorizationResult.Fail("You have no permission to delete this shared account."));
        }
    }

    public class CanDeleteSharedAccountAttribute : DiscordResourceAuthorizationAttribute<SharedAccount>
    {
        public CanDeleteSharedAccountAttribute() : base(typeof(CanDeleteSharedAccount)) { }
    }
}
