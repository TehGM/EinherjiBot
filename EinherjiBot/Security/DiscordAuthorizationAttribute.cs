using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace TehGM.EinherjiBot.Security
{
    public interface IBotAuthorizationPolicyAttribute
    {
        Type PolicyType { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class DiscordAuthorizationAttribute : PreconditionAttribute, IBotAuthorizationPolicyAttribute
    {
        public Type PolicyType { get; }

        public DiscordAuthorizationAttribute(Type policyType)
        {
            this.PolicyType = policyType;
        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            IAuthContext auth = services.GetRequiredService<IAuthContext>();
            if (auth.IsBanned)
                return PreconditionResult.FromError($"You're banned in {EinherjiInfo.Name} system.");

            IBotAuthorizationService authService = services.GetRequiredService<IBotAuthorizationService>();
            BotAuthorizationResult result = await authService.AuthorizeAsync(new[] { this.PolicyType }).ConfigureAwait(false);
            if (!result.Succeeded)
                return PreconditionResult.FromError(result.Reason ?? "You lack privileges to do this.");
            return PreconditionResult.FromSuccess();
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class DiscordResourceAuthorizationAttribute<TResource> : ParameterPreconditionAttribute, IBotAuthorizationPolicyAttribute
    {
        public Type PolicyType { get; }

        public DiscordResourceAuthorizationAttribute(Type policyType)
        {
            this.PolicyType = policyType;
        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, IParameterInfo parameterInfo, object value, IServiceProvider services)
        {
            if (value is not TResource resource)
                throw new InvalidOperationException($"Value {parameterInfo.Name} is not a {typeof(TResource).Name}.");

            IAuthContext auth = services.GetRequiredService<IAuthContext>();
            if (auth.IsBanned)
                return PreconditionResult.FromError($"You're banned in {EinherjiInfo.Name} system.");

            IBotAuthorizationService authService = services.GetRequiredService<IBotAuthorizationService>();
            BotAuthorizationResult result = await authService.AuthorizeAsync(resource, new[] { this.PolicyType }).ConfigureAwait(false);
            if (!result.Succeeded)
                return PreconditionResult.FromError(result.Reason ?? "You lack privileges to do this.");
            return PreconditionResult.FromSuccess();
        }
    }
}
