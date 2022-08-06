namespace TehGM.EinherjiBot.Security
{
    public interface IBotAuthorizationService
    {
        Task<BotAuthorizationResult> AuthorizeAsync(IEnumerable<Type> policies, CancellationToken cancellationToken = default);
        Task<BotAuthorizationResult> AuthorizeAsync<TResource>(TResource resource, IEnumerable<Type> policies, CancellationToken cancellationToken = default);
    }
}
