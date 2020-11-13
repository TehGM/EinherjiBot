using System.Threading.Tasks;
using Discord;

namespace TehGM.EinherjiBot
{
    public interface IHostedDiscordClient
    {
        IDiscordClient Client { get; }

        Task StartClientAsync();
        Task StopClientAsync();
    }
}
