namespace TehGM.EinherjiBot.Intel
{
    public interface IUserIntelProvider
    {
        Task<UserIntelContext> GetAsync(ulong userID, ulong? guildID, CancellationToken cancellationToken = default);
        Task UpdateIntelAsync(UserIntel intel, CancellationToken cancellationToken = default);
    }
}
