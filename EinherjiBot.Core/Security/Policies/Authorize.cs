namespace TehGM.EinherjiBot.Security.Policies
{
    public class Authorize : IDiscordAuthorizationPolicy
    {
        protected IAuthProvider AuthProvider { get; }
        protected IAuthContext Auth => this.AuthProvider.User;

        public Authorize(IAuthProvider authProvider)
        {
            this.AuthProvider = authProvider;
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
}
