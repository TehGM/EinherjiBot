﻿using Discord;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.MessageTriggers.Actions
{
    [BsonDiscriminator("MessageTriggers.Actions.BanUser", Required = true)]
    public class BanUserAction : IMessageTriggerAction
    {
        [BsonId]
        public Guid ID { get; }
        [BsonElement("delay"), BsonIgnoreIfNull]
        public TimeSpan? Delay { get; set; }
        [BsonElement("pruneDays"), BsonIgnoreIfDefault, BsonDefaultValue(0)]
        public int PruneDays { get; set; }
        [BsonElement("reason"), BsonIgnoreIfNull]
        public string CustomReason { get; set; }

        [BsonConstructor(nameof(ID))]
        private BanUserAction(Guid id)
        {
            this.ID = id;
        }

        public BanUserAction(TimeSpan? delay)
            : this(Guid.NewGuid())
        {
            this.Delay = delay;
        }

        public BanUserAction()
            : this(null) { }

        public Task ExecuteAsync(MessageTrigger trigger, IMessage message, IServiceProvider services, CancellationToken cancellationToken = default)
        {
            if (message.Channel is not IGuildChannel channel)
                throw new InvalidOperationException($"{nameof(KickUserAction)} action can only be used for guild messages.");

            IDiscordAuthContext auth = services.GetRequiredService<IDiscordAuthContext>();
            string reason = this.CustomReason ?? $"{EinherjiInfo.Name} message trigger.";

            if (this.Delay == null || this.Delay <= TimeSpan.Zero)
                return auth.DiscordGuildUser.BanAsync(this.PruneDays, reason, cancellationToken.ToRequestOptions());

            _ = Task.Run(async () =>
            {
                await Task.Delay(this.Delay.Value).ConfigureAwait(false);
                await auth.DiscordGuildUser.BanAsync(this.PruneDays, reason, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            }, 
                cancellationToken);
            return Task.CompletedTask;
        }
    }
}
