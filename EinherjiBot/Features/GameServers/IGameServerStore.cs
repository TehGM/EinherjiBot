namespace TehGM.EinherjiBot.GameServers
{
    public interface IGameServerStore
    {
        Task<GameServer> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<GameServer>> FindAsync(bool? isPublic, ulong? userID, IEnumerable<ulong> roleIDs, CancellationToken cancellationToken = default);

        Task UpdateAsync(GameServer server, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
