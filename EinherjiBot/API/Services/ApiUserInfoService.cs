using Discord;
using TehGM.EinherjiBot.DiscordClient;

namespace TehGM.EinherjiBot.API.Services
{
    public class ApiUserInfoService : IUserInfoService
    {
        private readonly IDiscordClient _client;
        private readonly IDiscordConnection _connection;

        public ApiUserInfoService(IDiscordClient client, IDiscordConnection connection)
        {
            this._client = client;
            this._connection = connection;
        }

        public async Task<UserInfoResponse> GetBotInfoAsync(CancellationToken cancellationToken = default)
        {
            await this._connection.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            return CreateUserInfo(this._client.CurrentUser);
        }

        public async Task<UserInfoResponse> GetUserInfoAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            await this._connection.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            IUser user = await this._client.GetUserAsync(userID, cancellationToken).ConfigureAwait(false);
            if (user == null)
                return null;
            return CreateUserInfo(user);
        }

        private static UserInfoResponse CreateUserInfo(IUser user)
            => new UserInfoResponse(user.Id, user.Username, user.Discriminator, user.AvatarId);
    }
}
