using Microsoft.Extensions.DependencyInjection;

namespace TehGM.EinherjiBot.Security.Services
{
    public class DiscordAuthorizationService : IDiscordAuthorizationService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _log;

        public DiscordAuthorizationService(IServiceProvider services, ILogger<DiscordAuthorizationService> log)
        {
            this._services = services;
            this._log = log;
        }

        public async Task<DiscordAuthorizationResult> AuthorizeAsync(IEnumerable<Type> policies, CancellationToken cancellationToken = default)
        {
            if (policies?.Any() != true)
                return DiscordAuthorizationResult.Success;
            this._log.LogDebug("Authorizing policies {Policies}", string.Join(',', policies.Select(p => p.Name)));
            foreach (Type policy in policies)
            {
                DiscordAuthorizationResult policyResult = await this.ProcessPolicyAsync(policy, cancellationToken).ConfigureAwait(false);
                if (!policyResult.Succeeded)
                    return policyResult;
            }
            return DiscordAuthorizationResult.Success;
        }

        public async Task<DiscordAuthorizationResult> AuthorizeAsync<TResource>(TResource resource, IEnumerable<Type> policies, CancellationToken cancellationToken = default)
        {
            if (policies?.Any() != true)
                return DiscordAuthorizationResult.Success;
            this._log.LogDebug("Authorizing policies {Policies} against resource {Type} {Resource}", string.Join(',', policies.Select(p => p.Name)), typeof(TResource).Name, resource.ToString());
            foreach (Type policy in policies)
            {
                DiscordAuthorizationResult policyResult = await this.ProcessPolicyAsync(resource, policy, cancellationToken).ConfigureAwait(false);
                if (!policyResult.Succeeded)
                    return policyResult;
            }
            return DiscordAuthorizationResult.Success;
        }

        private Task<DiscordAuthorizationResult> ProcessPolicyAsync(Type policy, CancellationToken cancellationToken = default)
        {
            object policyInstance = ActivatorUtilities.CreateInstance(this._services, policy);
            if (policyInstance is IDiscordAuthorizationPolicy p)
                return p.EvaluateAsync(cancellationToken);
            throw new ArgumentException($"Policy doesn't implement {nameof(IDiscordAuthorizationPolicy)} interface.");
        }

        private Task<DiscordAuthorizationResult> ProcessPolicyAsync<TResource>(TResource resource, Type policy, CancellationToken cancellationToken = default)
        {
            object policyInstance = ActivatorUtilities.CreateInstance(this._services, policy);
            if (policyInstance is IDiscordAuthorizationPolicy<TResource> pg)
                return pg.EvaluateAsync(resource, cancellationToken);
            if (policyInstance is IDiscordAuthorizationPolicy p)
                return p.EvaluateAsync(cancellationToken);

            throw new ArgumentException($"Policy doesn't implement {nameof(IDiscordAuthorizationPolicy)} or {nameof(IDiscordAuthorizationPolicy<TResource>)} interface.");
        }
    }
}
