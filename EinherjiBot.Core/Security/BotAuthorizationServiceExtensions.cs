namespace TehGM.EinherjiBot.Security
{
    public static class BotAuthorizationServiceExtensions
    {
        public static Task<BotAuthorizationResult> AuthorizeAsync(this IBotAuthorizationService service, Type policy, CancellationToken cancellationToken = default)
            => service.AuthorizeAsync(new[] { policy }, cancellationToken);
        public static Task<BotAuthorizationResult> AuthorizeAsync<TResource>(this IBotAuthorizationService service, TResource resource, Type policy, CancellationToken cancellationToken = default)
            => service.AuthorizeAsync(resource, new[] { policy }, cancellationToken);
    }
}
