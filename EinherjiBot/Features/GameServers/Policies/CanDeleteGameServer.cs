using TehGM.EinherjiBot.Security.Authorization;

namespace TehGM.EinherjiBot.GameServers.Policies
{
    public class CanDeleteGameServer : IDiscordAuthorizationPolicy<GameServer>
    {
        private readonly IDiscordAuthContext _auth;

        public CanDeleteGameServer(IDiscordAuthContext auth)
        {
            this._auth = auth;
        }

        public Task<DiscordAuthorizationResult> EvaluateAsync(GameServer resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.IsAdmin())
                return Task.FromResult(DiscordAuthorizationResult.Success);
            return Task.FromResult(DiscordAuthorizationResult.Fail("You have no permission to delete this game server."));
        }
    }

    public class CanDeleteGameServerAttribute : DiscordResourceAuthorizationAttribute<GameServer>
    {
        public CanDeleteGameServerAttribute() : base(typeof(CanDeleteGameServer)) { }
    }
}
