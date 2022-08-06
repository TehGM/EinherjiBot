namespace TehGM.EinherjiBot.Security.Policies
{
    public class AuthorizeBotOrAdmin : Authorize, IBotAuthorizationPolicy
    {
        public AuthorizeBotOrAdmin(IAuthProvider authProvider) : base(authProvider) { }

        public override async Task<BotAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            BotAuthorizationResult result = await base.EvaluateAsync(cancellationToken).ConfigureAwait(false);
            if (!result.Succeeded)
                return result;

            if (!base.Auth.IsAdmin() && !base.Auth.HasRole(UserRole.EinherjiBot))
                return BotAuthorizationResult.Fail($"You're not a {EinherjiInfo.Name} admin.");

            return BotAuthorizationResult.Success;
        }
    }
}
