using Discord;

namespace TehGM.EinherjiBot.DiscordClient
{
    public interface IHostedDiscordClient
    {
        IDiscordClient Client { get; }

        Task StartClientAsync();
        Task StopClientAsync();
    }
}
