using System.Threading;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.GameServers
{
    public interface IGameServerStore
    {
        Task<GameServer> GetAsync(string name, CancellationToken cancellationToken = default);
    }
}
