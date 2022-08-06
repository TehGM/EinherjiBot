using TehGM.EinherjiBot.Security.Authorization;

namespace TehGM.EinherjiBot.Security.Policies
{
    public class AuthorizeBotOrAdmin : Authorize, IDiscordAuthorizationPolicy
    {
        public AuthorizeBotOrAdmin(IAuthProvider authProvider) : base(authProvider) { }

        public override async Task<DiscordAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            DiscordAuthorizationResult result = await base.EvaluateAsync(cancellationToken).ConfigureAwait(false);
            if (!result.Succeeded)
                return result;

            if (!base.Auth.IsAdmin() && !base.Auth.HasRole(UserRole.EinherjiBot))
                return DiscordAuthorizationResult.Fail($"You're not a {EinherjiInfo.Name} admin.");

            return DiscordAuthorizationResult.Success;
        }
    }
}
