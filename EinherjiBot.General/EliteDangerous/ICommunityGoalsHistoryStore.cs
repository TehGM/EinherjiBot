using System.Threading;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.EliteDangerous
{
    public interface ICommunityGoalsHistoryStore
    {
        Task<CommunityGoal> GetAsync(int id, CancellationToken cancellationToken = default);
        Task SetAsync(CommunityGoal cg, CancellationToken cancellationToken = default);
    }
}
