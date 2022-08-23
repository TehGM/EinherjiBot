namespace TehGM.EinherjiBot.GameServers.Policies
{
    public class CanAccessGameServer : IBotAuthorizationPolicy<GameServer>
    {
        private readonly IDiscordAuthContext _auth;

        public CanAccessGameServer(IDiscordAuthContext auth)
        {
            this._auth = auth;
        }

        public Task<BotAuthorizationResult> EvaluateAsync(GameServer resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.IsAdmin() || resource.IsPublic
                || resource.AuthorizedUserIDs.Contains(this._auth.ID)
                || resource.AuthorizedRoleIDs.Intersect(this._auth.RecognizedDiscordRoleIDs).Any())
                return Task.FromResult(BotAuthorizationResult.Success);
            return Task.FromResult(BotAuthorizationResult.Fail("You have no permission to access this game server."));
        }
    }

    public class CanAccessGameServerAttribute : DiscordResourceAuthorizationAttribute<GameServer>
    {
        public CanAccessGameServerAttribute() : base(typeof(CanAccessGameServer)) { }
    }
}
