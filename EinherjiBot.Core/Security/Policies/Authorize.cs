namespace TehGM.EinherjiBot.Security.Policies
{
    public class Authorize : IBotAuthorizationPolicy
    {
        protected IAuthProvider AuthProvider { get; }
        protected IAuthContext Auth => this.AuthProvider.User;

        public Authorize(IAuthProvider authProvider)
        {
            this.AuthProvider = authProvider;
        }

        public virtual Task<BotAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            if (!this.Auth.IsLoggedIn())
                return Task.FromResult(BotAuthorizationResult.Fail("Not logged in."));
            if (this.Auth.IsBanned)
                return Task.FromResult(BotAuthorizationResult.Fail($"You're banned in {EinherjiInfo.Name} system."));
            return Task.FromResult(BotAuthorizationResult.Success);
        }
    }
}
