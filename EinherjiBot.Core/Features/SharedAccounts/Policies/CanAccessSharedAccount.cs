using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.SharedAccounts.Policies
{
    public class CanAccessSharedAccount : IDiscordAuthorizationPolicy<ISharedAccount>
    {
        private readonly IAuthContext _auth;

        public CanAccessSharedAccount(IAuthContext auth)
        {
            this._auth = auth;
        }

        public Task<DiscordAuthorizationResult> EvaluateAsync(ISharedAccount resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.IsAdmin()
                || resource.AuthorizedUserIDs?.Contains(this._auth.ID) == true
                || resource.AuthorizedRoleIDs?.Intersect(this._auth.KnownDiscordRoleIDs).Any() == true)
                return Task.FromResult(DiscordAuthorizationResult.Success);
            return Task.FromResult(DiscordAuthorizationResult.Fail("You have no permission to access this shared account."));
        }
    }
}
