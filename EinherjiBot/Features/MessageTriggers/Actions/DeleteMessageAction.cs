using Discord;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.MessageTriggers.Actions
{
    public class DeleteMessageAction : IMessageTriggerAction
    {
        [BsonId]
        public Guid ID { get; }
        [BsonElement("delay"), BsonIgnoreIfNull]
        public TimeSpan? Delay { get; set; }

        [BsonConstructor(nameof(ID))]
        private DeleteMessageAction(Guid id)
        {
            this.ID = id;
        }

        public DeleteMessageAction(TimeSpan? delay)
            : this(Guid.NewGuid())
        {
            this.Delay = delay;
        }

        public DeleteMessageAction()
            : this(null) { }

        public Task ExecuteAsync(MessageTrigger trigger, IMessage message, IServiceProvider services, CancellationToken cancellationToken = default)
        {
            if (this.Delay == null || this.Delay <= TimeSpan.Zero)
                return message.DeleteAsync(cancellationToken);

            _ = Task.Run(async () =>
            {
                await Task.Delay(this.Delay.Value).ConfigureAwait(false);
                await message.DeleteAsync(cancellationToken).ConfigureAwait(false);
            }, 
                cancellationToken);
            return Task.CompletedTask;
        }
    }
}
