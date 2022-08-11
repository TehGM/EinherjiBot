using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.UI.Security.Policies
{
    public class HasSharedAccountsFeature : IBotAuthorizationPolicy
    {
        private readonly IWebAuthProvider _provider;

        public HasSharedAccountsFeature(IWebAuthProvider provider)
        {
            this._provider = provider;
        }

        public Task<BotAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            if (this._provider.UserFeatures.Contains(UserFeature.SharedAccounts))
                return Task.FromResult(BotAuthorizationResult.Success);

            return Task.FromResult(BotAuthorizationResult.Fail("You have no access to Shared Accounts feature"));
        }
    }

    public class HasSharedAccountsFeatureAttribute : AuthorizeAttribute, IBotAuthorizationPolicyAttribute
    {
        public HasSharedAccountsFeatureAttribute() : base(typeof(HasSharedAccountsFeature)) { }
    }
}
