using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.SharedAccounts.Policies
{
    public class CanAccessSharedAccount : IDiscordAuthorizationPolicy<ISharedAccount>
    {
        private readonly IAuthProvider _auth;

        public CanAccessSharedAccount(IAuthProvider auth)
        {
            this._auth = auth;
        }

        public Task<BotAuthorizationResult> EvaluateAsync(ISharedAccount resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.User.IsAdmin()
                || resource.AuthorizedUserIDs?.Contains(this._auth.User.ID) == true
                || resource.AuthorizedRoleIDs?.Intersect(this._auth.User.KnownDiscordRoleIDs).Any() == true)
                return Task.FromResult(BotAuthorizationResult.Success);
            return Task.FromResult(BotAuthorizationResult.Fail("You have no permission to access this shared account."));
        }
    }
}
