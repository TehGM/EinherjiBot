namespace TehGM.EinherjiBot.GameServers.Policies
{
    public class CanDeleteGameServer : IBotAuthorizationPolicy<GameServer>
    {
        private readonly IDiscordAuthContext _auth;

        public CanDeleteGameServer(IDiscordAuthContext auth)
        {
            this._auth = auth;
        }

        public Task<BotAuthorizationResult> EvaluateAsync(GameServer resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.IsAdmin())
                return Task.FromResult(BotAuthorizationResult.Success);
            return Task.FromResult(BotAuthorizationResult.Fail("You have no permission to delete this game server."));
        }
    }

    public class CanDeleteGameServerAttribute : DiscordResourceAuthorizationAttribute<GameServer>
    {
        public CanDeleteGameServerAttribute() : base(typeof(CanDeleteGameServer)) { }
    }
}
