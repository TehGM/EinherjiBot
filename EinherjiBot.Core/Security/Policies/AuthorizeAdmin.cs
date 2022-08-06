namespace TehGM.EinherjiBot.Security.Policies
{
    public class AuthorizeAdmin : Authorize, IDiscordAuthorizationPolicy
    {
        public AuthorizeAdmin(IAuthProvider authProvider) : base(authProvider) { }

        public override async Task<DiscordAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            DiscordAuthorizationResult result = await base.EvaluateAsync(cancellationToken).ConfigureAwait(false);
            if (!result.Succeeded)
                return result;

            if (!Auth.IsAdmin())
                return DiscordAuthorizationResult.Fail($"You're not a {EinherjiInfo.Name} admin.");

            return DiscordAuthorizationResult.Success;
        }
    }
}
