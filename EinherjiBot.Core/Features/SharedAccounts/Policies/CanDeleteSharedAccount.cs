using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.Authorization;

namespace TehGM.EinherjiBot.SharedAccounts.Policies
{
    public class CanDeleteSharedAccount : IDiscordAuthorizationPolicy<ISharedAccount>
    {
        private readonly IAuthContext _auth;

        public CanDeleteSharedAccount(IAuthContext auth)
        {
            this._auth = auth;
        }

        public Task<DiscordAuthorizationResult> EvaluateAsync(ISharedAccount resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.IsAdmin() || resource.ModUserIDs?.Contains(this._auth.ID) == true)
                return Task.FromResult(DiscordAuthorizationResult.Success);
            return Task.FromResult(DiscordAuthorizationResult.Fail("You have no permission to delete this shared account."));
        }
    }
}
