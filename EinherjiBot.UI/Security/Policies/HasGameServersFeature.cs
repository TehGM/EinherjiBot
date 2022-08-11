using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.UI.Security.Policies
{
    public class HasGameServersFeature : IBotAuthorizationPolicy
    {
        private readonly IWebAuthProvider _provider;

        public HasGameServersFeature(IWebAuthProvider provider)
        {
            this._provider = provider;
        }

        public Task<BotAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            if (this._provider.UserFeatures?.Contains(UserFeature.GameServers) == true)
                return Task.FromResult(BotAuthorizationResult.Success);

            return Task.FromResult(BotAuthorizationResult.Fail("You have no access to Game Servers feature"));
        }
    }

    public class HasGameServersFeatureAttribute : AuthorizeAttribute, IBotAuthorizationPolicyAttribute
    {
        public HasGameServersFeatureAttribute() : base(typeof(HasGameServersFeature)) { }
    }
}
