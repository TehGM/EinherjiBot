namespace TehGM.EinherjiBot.GameServers.Policies
{
    public class CanAccessGameServer : IDiscordAuthorizationPolicy<GameServer>
    {
        private readonly IDiscordAuthContext _auth;

        public CanAccessGameServer(IDiscordAuthContext auth)
        {
            this._auth = auth;
        }

        public Task<DiscordAuthorizationResult> EvaluateAsync(GameServer resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.IsAdmin() || resource.IsPublic
                || resource.AuthorizedUserIDs.Contains(this._auth.ID)
                || resource.AuthorizedRoleIDs.Intersect(this._auth.KnownDiscordRoleIDs).Any())
                return Task.FromResult(DiscordAuthorizationResult.Success);
            return Task.FromResult(DiscordAuthorizationResult.Fail("You have no permission to access this game server."));
        }
    }

    public class CanAccessGameServerAttribute : DiscordResourceAuthorizationAttribute<GameServer>
    {
        public CanAccessGameServerAttribute() : base(typeof(CanAccessGameServer)) { }
    }
}
