using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.Authorization;
using TehGM.EinherjiBot.Security.Authorization.Policies;

namespace TehGM.EinherjiBot.SharedAccounts.Policies
{
    public class CanCreateSharedAccount : Authorize
    {
        public CanCreateSharedAccount(IAuthProvider authProvider) : base(authProvider) { }

        public override async Task<DiscordAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            DiscordAuthorizationResult result = await base.EvaluateAsync(cancellationToken).ConfigureAwait(false);
            if (!result.Succeeded)
                return result;

            if (!base.Auth.IsAdmin() && !base.Auth.HasRole(UserRole.SharedAccountCreator))
                return DiscordAuthorizationResult.Fail("You have no permissions to create game servers");

            return DiscordAuthorizationResult.Success;
        }
    }
}
