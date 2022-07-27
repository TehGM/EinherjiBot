namespace TehGM.EinherjiBot.Security.API
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string accessCode, CancellationToken cancellationToken = default);
        Task<LoginResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
