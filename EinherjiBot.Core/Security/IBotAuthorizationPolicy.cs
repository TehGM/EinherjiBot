namespace TehGM.EinherjiBot.Security
{
    public interface IBotAuthorizationPolicy
    {
        Task<BotAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default);
    }

    public interface IBotAuthorizationPolicy<in TResource>
    {
        Task<BotAuthorizationResult> EvaluateAsync(TResource resource, CancellationToken cancellationToken = default);
    }
}
