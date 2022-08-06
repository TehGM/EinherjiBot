using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.Policies;

namespace TehGM.EinherjiBot.SharedAccounts.Policies
{
    public class CanCreateSharedAccount : Authorize
    {
        public CanCreateSharedAccount(IAuthProvider authProvider) : base(authProvider) { }

        public override async Task<BotAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            BotAuthorizationResult result = await base.EvaluateAsync(cancellationToken).ConfigureAwait(false);
            if (!result.Succeeded)
                return result;

            if (!base.Auth.IsAdmin() && !base.Auth.HasRole(UserRole.SharedAccountCreator))
                return BotAuthorizationResult.Fail("You have no permissions to create game servers");

            return BotAuthorizationResult.Success;
        }
    }
}
