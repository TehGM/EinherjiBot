namespace TehGM.EinherjiBot.Security
{
    public interface IDiscordAuthProvider
    {
        Task<IDiscordAuthContext> GetAsync(ulong userID, ulong? guildID, CancellationToken cancellationToken = default);
        Task<UserSecurityData> GetUserSecurityDataAsync(ulong userID, CancellationToken cancellationToken = default);

        IDiscordAuthContext Current { get; set; }
    }
}
