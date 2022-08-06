namespace TehGM.EinherjiBot.Security
{
    public static class DiscordAuthorizationServiceExtensions
    {
        public static Task<DiscordAuthorizationResult> AuthorizeAsync(this IDiscordAuthorizationService service, Type policy, CancellationToken cancellationToken = default)
            => service.AuthorizeAsync(new[] { policy }, cancellationToken);
        public static Task<DiscordAuthorizationResult> AuthorizeAsync<TResource>(this IDiscordAuthorizationService service, TResource resource, Type policy, CancellationToken cancellationToken = default)
            => service.AuthorizeAsync(resource, new[] { policy }, cancellationToken);
    }
}
