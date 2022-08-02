using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.Authorization;
using TehGM.EinherjiBot.Security.Policies;

namespace TehGM.EinherjiBot.GameServers.Policies
{
    public class CanCreateGameServer : Authorize
    {
        public CanCreateGameServer(IAuthProvider authProvider) : base(authProvider) { }

        public override async Task<DiscordAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            DiscordAuthorizationResult result = await base.EvaluateAsync(cancellationToken).ConfigureAwait(false);
            if (!result.Succeeded)
                return result;

            if (!base.Auth.IsAdmin() && !base.Auth.HasRole(UserRole.GameServerCreator))
                return DiscordAuthorizationResult.Fail("You have no permissions to create game servers");

            return DiscordAuthorizationResult.Success;
        }
    }
}
