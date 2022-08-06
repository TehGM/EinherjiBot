namespace TehGM.EinherjiBot.Security.Policies
{
    public class AuthorizeAdmin : Authorize, IBotAuthorizationPolicy
    {
        public AuthorizeAdmin(IAuthProvider authProvider) : base(authProvider) { }

        public override async Task<BotAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            BotAuthorizationResult result = await base.EvaluateAsync(cancellationToken).ConfigureAwait(false);
            if (!result.Succeeded)
                return result;

            if (!Auth.IsAdmin())
                return BotAuthorizationResult.Fail($"You're not a {EinherjiInfo.Name} admin.");

            return BotAuthorizationResult.Success;
        }
    }
}
