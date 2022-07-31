using TehGM.EinherjiBot.Security.Authorization;

namespace TehGM.EinherjiBot.SharedAccounts.Policies
{
    public class CanAccessSharedAccount : IDiscordAuthorizationPolicy<SharedAccount>
    {
        private readonly IDiscordAuthContext _auth;

        public CanAccessSharedAccount(IDiscordAuthContext auth)
        {
            this._auth = auth;
        }

        public Task<DiscordAuthorizationResult> EvaluateAsync(SharedAccount resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.IsAdmin()
                || resource.AuthorizedUserIDs.Contains(this._auth.ID)
                || resource.AuthorizedRoleIDs.Intersect(this._auth.KnownDiscordRoleIDs).Any())
                return Task.FromResult(DiscordAuthorizationResult.Success);
            return Task.FromResult(DiscordAuthorizationResult.Fail("You have no permission to access this shared account."));
        }
    }

    public class CanAccessSharedAccountAttribute : DiscordResourceAuthorizationAttribute<SharedAccount>
    {
        public CanAccessSharedAccountAttribute() : base(typeof(CanAccessSharedAccount)) { }
    }
}
