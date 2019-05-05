using Discord.WebSocket;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    public interface ICommandProcessor
    {
        Task<bool> ProcessAsync(DiscordSocketClient client, SocketMessage message);
    }
}
