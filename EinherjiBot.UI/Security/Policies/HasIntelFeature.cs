using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.UI.Security.Policies
{
    public class HasIntelFeature : IBotAuthorizationPolicy
    {
        private readonly IWebAuthProvider _provider;

        public HasIntelFeature(IWebAuthProvider provider)
        {
            this._provider = provider;
        }

        public Task<BotAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            if (this._provider.UserFeatures.Contains(UserFeature.Intel))
                return Task.FromResult(BotAuthorizationResult.Success);

            return Task.FromResult(BotAuthorizationResult.Fail("You have no access to Intel feature"));
        }
    }

    public class HasIntelFeatureAttribute : AuthorizeAttribute
    {
        public HasIntelFeatureAttribute() : base(typeof(HasIntelFeature)) { }
    }
}
