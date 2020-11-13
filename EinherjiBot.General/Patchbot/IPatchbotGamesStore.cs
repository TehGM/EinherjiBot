using System.Threading;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.Patchbot
{
    public interface IPatchbotGamesStore
    {
        Task<PatchbotGame> GetAsync(string name, CancellationToken cancellationToken = default);
        Task SetAsync(PatchbotGame game, CancellationToken cancellationToken = default);
        Task DeleteAsync(PatchbotGame game, CancellationToken cancellationToken = default);
    }
}
