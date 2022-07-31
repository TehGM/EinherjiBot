using TehGM.EinherjiBot.Security.Authorization;

namespace TehGM.EinherjiBot.SharedAccounts.Policies
{
    public class CanEditSharedAccount : IDiscordAuthorizationPolicy<SharedAccount>
    {
        private readonly IDiscordAuthContext _auth;

        public CanEditSharedAccount(IDiscordAuthContext auth)
        {
            this._auth = auth;
        }

        public Task<DiscordAuthorizationResult> EvaluateAsync(SharedAccount resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.IsAdmin() || resource.ModUserIDs.Contains(this._auth.ID))
                return Task.FromResult(DiscordAuthorizationResult.Success);
            return Task.FromResult(DiscordAuthorizationResult.Fail("You have no permission to edit this shared account."));
        }
    }

    public class CanEditSharedAccountAttribute : DiscordResourceAuthorizationAttribute<SharedAccount>
    {
        public CanEditSharedAccountAttribute() : base(typeof(CanEditSharedAccount)) { }
    }
}
