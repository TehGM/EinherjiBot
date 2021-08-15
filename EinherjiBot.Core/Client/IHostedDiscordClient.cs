using System.Threading.Tasks;
using DSharpPlus;

namespace TehGM.EinherjiBot
{
    public interface IHostedDiscordClient
    {
        DiscordClient Client { get; }

        Task StartClientAsync();
        Task StopClientAsync();
    }
}
