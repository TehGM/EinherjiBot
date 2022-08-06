using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.UI.Security.Policies
{
    public class HasSharedAccountsFeature : IDiscordAuthorizationPolicy
    {
        private readonly IWebAuthProvider _provider;

        public HasSharedAccountsFeature(IWebAuthProvider provider)
        {
            this._provider = provider;
        }

        public Task<DiscordAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            if (this._provider.UserFeatures.Contains(UserFeature.SharedAccounts))
                return Task.FromResult(DiscordAuthorizationResult.Success);

            return Task.FromResult(DiscordAuthorizationResult.Fail("You have no access to Shared Accounts feature"));
        }
    }

    public class HasSharedAccountsFeatureAttribute : AuthorizeAttribute
    {
        public HasSharedAccountsFeatureAttribute() : base(typeof(HasSharedAccountsFeature)) { }
    }
}
