using Discord;
using TehGM.EinherjiBot.DiscordClient;

namespace TehGM.EinherjiBot.Settings.Policies
{
    public class CanManageGuildSettings : IBotAuthorizationPolicy<IGuildSettings>
    {
        private readonly IAuthProvider _auth;
        private readonly IDiscordClient _client;
        private readonly IDiscordConnection _connection;

        public CanManageGuildSettings(IAuthProvider auth, IDiscordClient client, IDiscordConnection connection)
        {
            this._auth = auth;
            this._client = client;
            this._connection = connection;
        }

        public async Task<BotAuthorizationResult> EvaluateAsync(IGuildSettings resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.User.IsAdmin() || this._auth.User.IsEinherji())
                return BotAuthorizationResult.Success;

            await this._connection.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            IGuild guild = await this._client.GetGuildAsync(resource.GuildID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (guild != null)
            {
                IGuildUser user = await guild.GetUserAsync(this._auth.User.ID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                if (user != null && (user.IsOwner() || user.GuildPermissions.Administrator))
                    return BotAuthorizationResult.Success;
            }

            return BotAuthorizationResult.Fail($"You have no permission to access settings for guild {resource.GuildID}.");
        }
    }
}
