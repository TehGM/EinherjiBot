using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.SharedAccounts.Policies
{
    public class CanEditSharedAccount : IDiscordAuthorizationPolicy<ISharedAccount>
    {
        private readonly IAuthContext _auth;

        public CanEditSharedAccount(IAuthContext auth)
        {
            this._auth = auth;
        }

        public Task<BotAuthorizationResult> EvaluateAsync(ISharedAccount resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.IsAdmin() || resource.ModUserIDs?.Contains(this._auth.ID) == true)
                return Task.FromResult(BotAuthorizationResult.Success);
            return Task.FromResult(BotAuthorizationResult.Fail("You have no permission to edit this shared account."));
        }
    }
}
