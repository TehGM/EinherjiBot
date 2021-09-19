using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.RandomStatus.Services
{
    public class AdvancedStatusConverter : IAdvancedStatusConverter
    {
        public Task<string> ConvertAsync(Status status, CancellationToken cancellationToken = default)
        {
            if (!status.IsAdvanced)
                return Task.FromResult(status.Text);

            StringBuilder result = new StringBuilder(status.Text);

            result.Replace(AdvancedStatusVariables.BotVersion, $"v{BotInfoUtility.GetVersion()}");

            return Task.FromResult(result.ToString());
        }
    }
}
