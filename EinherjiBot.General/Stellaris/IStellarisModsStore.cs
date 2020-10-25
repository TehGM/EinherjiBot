using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.Stellaris
{
    public interface IStellarisModsStore
    {
        Task<IEnumerable<StellarisMod>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(StellarisMod mod, CancellationToken cancellationToken = default);
        Task RemoveAsync(IEnumerable<StellarisMod> mods, CancellationToken cancellationToken = default);
    }
}
