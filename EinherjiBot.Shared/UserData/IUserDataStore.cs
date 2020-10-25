using System.Threading;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot
{
    public interface IUserDataStore
    {
        Task<UserData> GetAsync(ulong userID, CancellationToken cancellationToken = default);
        Task SetAsync(UserData data, CancellationToken cancellationToken = default);
    }
}
