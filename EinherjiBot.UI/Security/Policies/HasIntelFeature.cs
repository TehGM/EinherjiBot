using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.Authorization;

namespace TehGM.EinherjiBot.UI.Security.Policies
{
    public class HasIntelFeature : IDiscordAuthorizationPolicy
    {
        private readonly IWebAuthProvider _provider;

        public HasIntelFeature(IWebAuthProvider provider)
        {
            this._provider = provider;
        }

        public Task<DiscordAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            if (this._provider.UserFeatures.Contains(UserFeature.Intel))
                return Task.FromResult(DiscordAuthorizationResult.Success);

            return Task.FromResult(DiscordAuthorizationResult.Fail("You have no access to Intel feature"));
        }
    }

    public class HasIntelFeatureAttribute : AuthorizeAttribute
    {
        public HasIntelFeatureAttribute() : base(typeof(HasIntelFeature)) { }
    }
}
