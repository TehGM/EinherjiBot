using Microsoft.Extensions.DependencyInjection;

namespace TehGM.EinherjiBot.Security.Services
{
    public class BotAuthorizationService : IBotAuthorizationService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _log;

        public BotAuthorizationService(IServiceProvider services, ILogger<BotAuthorizationService> log)
        {
            this._services = services;
            this._log = log;
        }

        public async Task<BotAuthorizationResult> AuthorizeAsync(IEnumerable<Type> policies, CancellationToken cancellationToken = default)
        {
            if (policies?.Any() != true)
                return BotAuthorizationResult.Success;
            this._log.LogDebug("Authorizing policies {Policies}", string.Join(',', policies.Select(p => p.Name)));
            foreach (Type policy in policies)
            {
                BotAuthorizationResult policyResult = await this.ProcessPolicyAsync(policy, cancellationToken).ConfigureAwait(false);
                if (!policyResult.Succeeded)
                    return policyResult;
            }
            return BotAuthorizationResult.Success;
        }

        public async Task<BotAuthorizationResult> AuthorizeAsync<TResource>(TResource resource, IEnumerable<Type> policies, CancellationToken cancellationToken = default)
        {
            if (policies?.Any() != true)
                return BotAuthorizationResult.Success;
            this._log.LogDebug("Authorizing policies {Policies} against resource {Type} {Resource}", string.Join(',', policies.Select(p => p.Name)), typeof(TResource).Name, resource.ToString());
            foreach (Type policy in policies)
            {
                BotAuthorizationResult policyResult = await this.ProcessPolicyAsync(resource, policy, cancellationToken).ConfigureAwait(false);
                if (!policyResult.Succeeded)
                    return policyResult;
            }
            return BotAuthorizationResult.Success;
        }

        private Task<BotAuthorizationResult> ProcessPolicyAsync(Type policy, CancellationToken cancellationToken = default)
        {
            object policyInstance = ActivatorUtilities.CreateInstance(this._services, policy);
            if (policyInstance is IBotAuthorizationPolicy p)
                return p.EvaluateAsync(cancellationToken);
            throw new ArgumentException($"Policy {policy.Name} doesn't implement {nameof(IBotAuthorizationPolicy)} interface.");
        }

        private Task<BotAuthorizationResult> ProcessPolicyAsync<TResource>(TResource resource, Type policy, CancellationToken cancellationToken = default)
        {
            object policyInstance = ActivatorUtilities.CreateInstance(this._services, policy);
            if (policyInstance is IBotAuthorizationPolicy<TResource> pg)
                return pg.EvaluateAsync(resource, cancellationToken);
            if (policyInstance is IBotAuthorizationPolicy p)
                return p.EvaluateAsync(cancellationToken);

            throw new ArgumentException($"Policy {policy.Name} doesn't implement {nameof(IBotAuthorizationPolicy)} or {nameof(IBotAuthorizationPolicy<TResource>)} interface.");
        }
    }
}
