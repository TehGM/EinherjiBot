using Discord;

namespace TehGM.EinherjiBot.API.Services
{
    public class ApiUserInfoService : IUserInfoService
    {
        private readonly IDiscordClient _client;

        public ApiUserInfoService(IDiscordClient client)
        {
            this._client = client;
        }

        public Task<UserInfoResponse> GetBotInfoAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(CreateUserInfo(this._client.CurrentUser));

        public async Task<UserInfoResponse> GetUserInfoAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            IUser user = await this._client.GetUserAsync(userID, cancellationToken).ConfigureAwait(false);
            if (user == null)
                return null;
            return CreateUserInfo(user);
        }

        private static UserInfoResponse CreateUserInfo(IUser user)
            => new UserInfoResponse(user.Id, user.Username, user.Discriminator, user.AvatarId);
    }
}
