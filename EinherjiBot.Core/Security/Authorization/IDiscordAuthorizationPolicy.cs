namespace TehGM.EinherjiBot.Security.Authorization
{
    public interface IDiscordAuthorizationPolicy
    {
        Task<DiscordAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default);
    }

    public interface IDiscordAuthorizationPolicy<TResource>
    {
        Task<DiscordAuthorizationResult> EvaluateAsync(TResource resource, CancellationToken cancellationToken = default);
    }
}
