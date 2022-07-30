using Discord;

namespace TehGM.EinherjiBot.MessageTriggers
{
    public interface IMessageTriggerAction
    {
        Guid ID { get; }
        Task ExecuteAsync(MessageTrigger trigger, IMessage message, IServiceProvider services, CancellationToken cancellationToken = default);
    }
}
