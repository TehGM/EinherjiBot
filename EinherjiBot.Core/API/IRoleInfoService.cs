namespace TehGM.EinherjiBot.API
{
    public interface IRoleInfoService
    {
        Task<RoleInfoResponse> GetRoleInfoAsync(ulong id, CancellationToken cancellationToken = default);
    }
}
