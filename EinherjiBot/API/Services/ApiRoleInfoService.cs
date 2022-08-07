using Discord;
using TehGM.EinherjiBot.DiscordClient;

namespace TehGM.EinherjiBot.API.Services
{
    public class ApiRoleInfoService : IRoleInfoService
    {
        private readonly IDiscordClient _client;
        private readonly IDiscordConnection _connection;

        public ApiRoleInfoService(IDiscordClient client, IDiscordConnection connection)
        {
            this._client = client;
            this._connection = connection;
        }

        public async Task<RoleInfoResponse> GetRoleInfoAsync(ulong id, CancellationToken cancellationToken = default)
        {
            await this._connection.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            IEnumerable<IGuild> guilds = await this._client.GetGuildsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            foreach (IGuild guild in guilds)
            {
                IRole role = guild.GetRole(id);
                if (role == null)
                    continue;

                return new RoleInfoResponse(role.Id, role.Name, guild.Id, guild.Name, role.Color);
            }
            return null;
        }
    }
}
