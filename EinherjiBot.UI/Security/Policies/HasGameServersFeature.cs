using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.UI.Security.Policies
{
    public class HasGameServersFeature : IDiscordAuthorizationPolicy
    {
        private readonly IWebAuthProvider _provider;

        public HasGameServersFeature(IWebAuthProvider provider)
        {
            this._provider = provider;
        }

        public Task<DiscordAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            if (this._provider.UserFeatures.Contains(UserFeature.GameServers))
                return Task.FromResult(DiscordAuthorizationResult.Success);

            return Task.FromResult(DiscordAuthorizationResult.Fail("You have no access to Game Servers feature"));
        }
    }

    public class HasGameServersFeatureAttribute : AuthorizeAttribute
    {
        public HasGameServersFeatureAttribute() : base(typeof(HasGameServersFeature)) { }
    }
}
