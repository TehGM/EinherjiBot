namespace TehGM.EinherjiBot.Security.Authorization.Policies
{
    public class Authorize : IDiscordAuthorizationPolicy
    {
        protected IDiscordAuthContext Auth { get; }

        public Authorize(IDiscordAuthContext auth)
        {
            this.Auth = auth;
        }

        public virtual Task<DiscordAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            if (!this.Auth.IsLoggedIn())
                return Task.FromResult(DiscordAuthorizationResult.Fail("Not logged in."));
            if (this.Auth.IsBanned)
                return Task.FromResult(DiscordAuthorizationResult.Fail($"You're banned in {EinherjiInfo.Name} system."));
            return Task.FromResult(DiscordAuthorizationResult.Success);
        }
    }

    public class AuthorizeAttribute : DiscordAuthorizationAttribute
    {
        public AuthorizeAttribute() : base(typeof(Authorize)) { }
    }
}
