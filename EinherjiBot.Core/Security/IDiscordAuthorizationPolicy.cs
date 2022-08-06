namespace TehGM.EinherjiBot.Security
{
    public interface IDiscordAuthorizationPolicy
    {
        Task<DiscordAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default);
    }

    public interface IDiscordAuthorizationPolicy<in TResource>
    {
        Task<DiscordAuthorizationResult> EvaluateAsync(TResource resource, CancellationToken cancellationToken = default);
    }
}
