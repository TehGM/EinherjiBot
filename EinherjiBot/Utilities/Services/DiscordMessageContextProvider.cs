using Discord;

namespace TehGM.EinherjiBot.Services
{
    internal class DiscordMessageContextProvider : IMessageContextProvider
    {
        public IMessage Message { get; set; }
    }
}
