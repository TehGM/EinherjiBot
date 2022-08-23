using TehGM.EinherjiBot.GameServers;
using TehGM.EinherjiBot.SharedAccounts;

namespace TehGM.EinherjiBot.Security.API.Services
{
    // TODO: this currently queries DB each time and depends on auth context being already built
    // this is less than ideal, so think of how to improve that issue with auth system - and since context is dependency for a lot, it might mean a lot of changes required
    // also, this gives even more reason to rate-limit auth endpoints
    public class UserFeatureProvider : IUserFeatureProvider
    {
        private readonly ISharedAccountStore _sharedAccounts;
        private readonly IGameServerStore _gameServers;

        public UserFeatureProvider(ISharedAccountStore sharedAccounts, IGameServerStore gameServers)
        {
            this._sharedAccounts = sharedAccounts;
            this._gameServers = gameServers;
        }

        public async Task<IEnumerable<string>> GetForUserAsync(IDiscordAuthContext context, CancellationToken cancellationToken = default)
        {
            Task<bool> sharedAccountTask = this.CheckSharedAccountsAsync(context, cancellationToken);
            Task<bool> gameServersTask = this.CheckGameServersAsync(context, cancellationToken);

            List<string> features = new List<string>();
            features.Add(UserFeature.Intel);

            await Task.WhenAll(sharedAccountTask, gameServersTask).ConfigureAwait(false);
            if (sharedAccountTask.Result)
                features.Add(UserFeature.SharedAccounts);
            if (gameServersTask.Result)
                features.Add(UserFeature.GameServers);

            return features.ToArray();
        }

        private async Task<bool> CheckSharedAccountsAsync(IDiscordAuthContext context, CancellationToken cancellationToken)
        {
            if (context.IsAdmin() || context.HasRole(UserRole.SharedAccountCreator))
                return true;
            IEnumerable<SharedAccount> foundResources = await this._sharedAccounts.FindAsync(null, context.ID, context.RecognizedDiscordRoleIDs, false, cancellationToken).ConfigureAwait(false);
            return foundResources?.Any() == true;
        }

        private async Task<bool> CheckGameServersAsync(IDiscordAuthContext context, CancellationToken cancellationToken)
        {
            if (context.IsAdmin() || context.HasRole(UserRole.GameServerCreator))
                return true;
            IEnumerable<GameServer> foundResources = await this._gameServers.FindAsync(null, context.ID, context.RecognizedDiscordRoleIDs, cancellationToken).ConfigureAwait(false);
            return foundResources?.Any() == true;
        }
    }
}
