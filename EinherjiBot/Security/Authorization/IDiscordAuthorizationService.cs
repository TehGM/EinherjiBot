namespace TehGM.EinherjiBot.Security.Authorization
{
    public interface IDiscordAuthorizationService
    {
        Task<DiscordAuthorizationResult> AuthorizeAsync(IEnumerable<Type> policies, CancellationToken cancellationToken = default);
        Task<DiscordAuthorizationResult> AuthorizeAsync<TResource>(TResource resource, IEnumerable<Type> policies, CancellationToken cancellationToken = default);
    }
}
