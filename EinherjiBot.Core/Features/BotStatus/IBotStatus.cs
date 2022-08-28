using Discord;

namespace TehGM.EinherjiBot.BotStatus
{
    public interface IBotStatus
    {
        public Guid ID { get; }
        public string Text { get; }
        public string Link { get; }
        public ActivityType ActivityType { get; }
        public bool IsEnabled { get; }
        public IErrorInfo LastError { get; }
    }
}
