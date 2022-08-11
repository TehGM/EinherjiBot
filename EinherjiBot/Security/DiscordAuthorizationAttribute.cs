using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace TehGM.EinherjiBot.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class DiscordAuthorizationAttribute : PreconditionAttribute, IBotAuthorizationPolicyAttribute
    {
        public IEnumerable<Type> PolicyTypes { get; }

        public DiscordAuthorizationAttribute(params Type[] policies)
        {
            this.PolicyTypes = AuthorizationPolicyHelper.AppendAuthorizePolicy(policies);
        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            IBotAuthorizationService authService = services.GetRequiredService<IBotAuthorizationService>();
            BotAuthorizationResult result = await authService.AuthorizeAsync(this.PolicyTypes).ConfigureAwait(false);
            if (!result.Succeeded)
                return PreconditionResult.FromError(result.Reason ?? "You lack privileges to do this.");
            return PreconditionResult.FromSuccess();
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class DiscordResourceAuthorizationAttribute<TResource> : ParameterPreconditionAttribute, IBotAuthorizationPolicyAttribute
    {
        public IEnumerable<Type> PolicyTypes { get; }

        public DiscordResourceAuthorizationAttribute(params Type[] policies)
        {
            this.PolicyTypes = AuthorizationPolicyHelper.AppendAuthorizePolicy(policies);
        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, IParameterInfo parameterInfo, object value, IServiceProvider services)
        {
            if (value is not TResource resource)
                throw new InvalidOperationException($"Value {parameterInfo.Name} is not a {typeof(TResource).Name}.");

            IBotAuthorizationService authService = services.GetRequiredService<IBotAuthorizationService>();
            BotAuthorizationResult result = await authService.AuthorizeAsync(resource, this.PolicyTypes).ConfigureAwait(false);
            if (!result.Succeeded)
                return PreconditionResult.FromError(result.Reason ?? "You lack privileges to do this.");
            return PreconditionResult.FromSuccess();
        }
    }
}
