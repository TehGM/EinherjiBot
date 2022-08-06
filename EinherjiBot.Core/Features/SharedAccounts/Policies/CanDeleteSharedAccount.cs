using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.SharedAccounts.Policies
{
    public class CanDeleteSharedAccount : IBotAuthorizationPolicy<ISharedAccount>
    {
        private readonly IAuthProvider _auth;

        public CanDeleteSharedAccount(IAuthProvider auth)
        {
            this._auth = auth;
        }

        public Task<BotAuthorizationResult> EvaluateAsync(ISharedAccount resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.User.IsAdmin() || resource.ModUserIDs?.Contains(this._auth.User.ID) == true)
                return Task.FromResult(BotAuthorizationResult.Success);
            return Task.FromResult(BotAuthorizationResult.Fail("You have no permission to delete this shared account."));
        }
    }
}
