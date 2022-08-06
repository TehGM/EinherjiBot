namespace TehGM.EinherjiBot.Security
{
    public interface IBotAuthorizationPolicy
    {
        Task<BotAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default);
    }

    public interface IDiscordAuthorizationPolicy<in TResource>
    {
        Task<BotAuthorizationResult> EvaluateAsync(TResource resource, CancellationToken cancellationToken = default);
    }
}
