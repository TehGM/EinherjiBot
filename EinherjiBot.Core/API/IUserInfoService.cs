namespace TehGM.EinherjiBot.API
{
    public interface IUserInfoService
    {
        Task<UserInfoResponse> GetBotInfoAsync(CancellationToken cancellationToken = default);
        Task<UserInfoResponse> GetUserInfoAsync(ulong userID, CancellationToken cancellationToken = default);
    }
}
