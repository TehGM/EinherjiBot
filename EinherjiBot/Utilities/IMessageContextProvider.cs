using Discord;

namespace TehGM.EinherjiBot
{
    public interface IMessageContextProvider
    {
        IMessage Message { get; set; }
    }
}
