using Discord;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Attributes;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.Auditing.MessageTriggers;
using TehGM.EinherjiBot.PlaceholdersEngine;

namespace TehGM.EinherjiBot.MessageTriggers.Actions
{
    public class AuditAction : IMessageTriggerAction
    {
        [BsonId]
        public Guid ID { get; }
        [BsonElement("lifetime")]
        public TimeSpan? Lifetime { get; set; }

        [BsonConstructor(nameof(ID), nameof(Lifetime))]
        private AuditAction(Guid id, TimeSpan? lifetime)
        {
            this.ID = id;
            this.Lifetime = lifetime;
        }

        public AuditAction(TimeSpan lifetime)
            : this(Guid.NewGuid(), lifetime) { }

        public AuditAction()
            : this(BotAuditEntry.DefaultExpiration) { }

        public async Task ExecuteAsync(MessageTrigger trigger, IMessage message, IServiceProvider services, CancellationToken cancellationToken = default)
        {
            IPlaceholdersEngine placeholders = services.GetRequiredService<IPlaceholdersEngine>();
            PlaceholderConvertContext context = new PlaceholderConvertContext(trigger.PlaceholderContext)
            {
                CurrentUserID = message.Author.Id,
                CurrentChannelID = message.Channel.Id,
                CurrentGuildID = (message.Channel as IGuildChannel)?.GuildId,
                MessageContent = message.Content
            };
            string text = await placeholders.ConvertPlaceholdersAsync(message.Content, context, services, cancellationToken).ConfigureAwait(false);
            MessageTriggerAuditEntry entry = new MessageTriggerAuditEntry(message, text, this.Lifetime);

            IAuditStore<MessageTriggerAuditEntry> audit = services.GetRequiredService<IAuditStore<MessageTriggerAuditEntry>>();
            await audit.AddAuditAsync(entry, cancellationToken).ConfigureAwait(false);
        }
    }
}
