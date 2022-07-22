namespace TehGM.EinherjiBot.GameServers
{
    public interface IGameServerProvider
    {
        Task<GameServer> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<GameServer>> GetAllAsync(CancellationToken cancellationToken = default);

        Task UpdateAsync(GameServer server, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
