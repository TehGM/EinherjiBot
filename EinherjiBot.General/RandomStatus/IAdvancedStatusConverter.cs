using System.Threading;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.RandomStatus
{
    public interface IAdvancedStatusConverter
    {
        Task<string> ConvertAsync(Status status, CancellationToken cancellationToken = default);
    }
}
