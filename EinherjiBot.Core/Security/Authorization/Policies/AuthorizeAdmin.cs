namespace TehGM.EinherjiBot.Security.Authorization.Policies
{
    public class AuthorizeAdmin : Authorize, IDiscordAuthorizationPolicy
    {
        public AuthorizeAdmin(IAuthContext auth) : base(auth) { }

        public override async Task<DiscordAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            DiscordAuthorizationResult result = await base.EvaluateAsync(cancellationToken).ConfigureAwait(false);
            if (!result.Succeeded)
                return result;

            if (!base.Auth.IsAdmin())
                return DiscordAuthorizationResult.Fail($"You're not a {EinherjiInfo.Name} admin.");

            return DiscordAuthorizationResult.Success;
        }
    }
}
