namespace TehGM.EinherjiBot.Security
{
    public interface IDiscordAuthProvider : IAuthProvider
    {
        Task<IDiscordAuthContext> GetAsync(ulong userID, ulong? guildID, CancellationToken cancellationToken = default);
        Task<UserSecurityData> GetUserSecurityDataAsync(ulong userID, CancellationToken cancellationToken = default);
        Task<IDiscordAuthContext> GetBotContextAsync(CancellationToken cancellationToken = default);

        new IDiscordAuthContext User { get; set; }
    }
}
