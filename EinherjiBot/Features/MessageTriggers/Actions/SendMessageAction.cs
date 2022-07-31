using Discord;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Attributes;
using TehGM.EinherjiBot.PlaceholdersEngine;

namespace TehGM.EinherjiBot.MessageTriggers.Actions
{
    [BsonDiscriminator("MessageTriggers.Actions.SendMessage", Required = true)]
    public class SendMessageAction : IMessageTriggerAction
    {
        [BsonId]
        public Guid ID { get; }
        [BsonElement("text")]
        public string Text { get; set; }
        [BsonElement("channel"), BsonIgnoreIfNull, BsonDefaultValue(null)]
        public ulong? ChannelID { get; set; }
        [BsonElement("disableMentions"), BsonDefaultValue(false), BsonIgnoreIfDefault]
        public bool DisableMentions { get; set; }

        [BsonConstructor(nameof(ID))]
        private SendMessageAction(Guid id)
        {
            this.ID = id;
        }

        public SendMessageAction(string text)
            : this(Guid.NewGuid())
        {
            this.Text = text;
        }

        public async Task ExecuteAsync(MessageTrigger trigger, IMessage message, IServiceProvider services, CancellationToken cancellationToken = default)
        {
            IPlaceholdersEngine placeholders = services.GetRequiredService<IPlaceholdersEngine>();
            ulong channelID = this.ChannelID ?? message.Channel.Id;
            string text = await placeholders.ConvertPlaceholdersAsync(this.Text, cancellationToken).ConfigureAwait(false);

            AllowedMentions allowedMentions = this.DisableMentions ? AllowedMentions.None : AllowedMentions.All;

            IDiscordClient client = services.GetRequiredService<IDiscordClient>();
            IChannel channel = await client.GetChannelAsync(channelID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (channel is not IMessageChannel messageChannel)
                return;
            await messageChannel.SendMessageAsync(text,
                allowedMentions: allowedMentions,
                options: cancellationToken.ToRequestOptions()).ConfigureAwait(false);
        }
    }
}
