namespace TehGM.EinherjiBot.GameServers.Policies
{
    public class CanEditGameServer : IBotAuthorizationPolicy<GameServer>
    {
        private readonly IDiscordAuthContext _auth;

        public CanEditGameServer(IDiscordAuthContext auth)
        {
            this._auth = auth;
        }

        public Task<BotAuthorizationResult> EvaluateAsync(GameServer resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.IsAdmin())
                return Task.FromResult(BotAuthorizationResult.Success);
            return Task.FromResult(BotAuthorizationResult.Fail("You have no permission to edit this game server."));
        }
    }

    public class CanEditGameServerAttribute : DiscordResourceAuthorizationAttribute<GameServer>
    {
        public CanEditGameServerAttribute() : base(typeof(CanEditGameServer)) { }
    }
}
