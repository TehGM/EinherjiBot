﻿using Discord;
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
        [BsonElement("embed"), BsonIgnoreIfNull]
        public EmbedInfo Embed { get; set; }
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
            IDiscordClient client = services.GetRequiredService<IDiscordClient>();
            ulong channelID = this.ChannelID ?? message.Channel.Id;
            IChannel channel = await client.GetChannelAsync(channelID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (channel is not IMessageChannel messageChannel)
                return;

            IPlaceholdersEngine placeholders = services.GetRequiredService<IPlaceholdersEngine>();
            string text = await placeholders.ConvertPlaceholdersAsync(this.Text, cancellationToken).ConfigureAwait(false);

            Embed embed = await this.Embed?.BuildAsync(message.Author, placeholders, client, services, cancellationToken);

            AllowedMentions allowedMentions = this.DisableMentions ? AllowedMentions.None : AllowedMentions.All;
            await messageChannel.SendMessageAsync(text,
                allowedMentions: allowedMentions,
                embed: embed,
                options: cancellationToken.ToRequestOptions()).ConfigureAwait(false);
        }

        public class EmbedInfo
        {
            [BsonElement("description")]
            public string Description { get; set; }
            [BsonElement("title")]
            public string Title { get; set; }
            [BsonElement("color")]
            public uint? Color { get; set; }
            [BsonElement("url"), BsonIgnoreIfNull]
            public string URL { get; set; }
            [BsonElement("image"), BsonIgnoreIfNull]
            public string ImageURL { get; set; }

            [BsonElement("fields"), BsonIgnoreIfDefault, BsonIgnoreIfNull, BsonDefaultValue(null)]
            public ICollection<FieldInfo> Fields { get; }

            [BsonElement("author"), BsonIgnoreIfNull]
            public UserInfo Author { get; set; }
            [BsonElement("authorID"), BsonIgnoreIfNull]
            public ulong? AuthorID { get; set; }
            [BsonElement("currentUserAuthor"), BsonIgnoreIfDefault, BsonDefaultValue(false)]
            public bool UseCurrentUserAsAuthor { get; set; }

            [BsonElement("footer")]
            public FooterInfo Footer { get; set; }

            [BsonElement("thumbnail"), BsonIgnoreIfNull]
            public string ThumbnailURL { get; set; }
            [BsonElement("currentUserThumbnail"), BsonIgnoreIfDefault, BsonDefaultValue(false)]
            public bool UseCurrentUserAsThumbnail { get; set; }

            [BsonElement("timestamp")]
            public DateTime? Timestamp { get; set; }
            [BsonElement("currentTimestamp"), BsonIgnoreIfDefault, BsonDefaultValue(false)]
            public bool UseCurrentTimestamp { get; set; }

            [BsonConstructor(nameof(Fields))]
            public EmbedInfo(IEnumerable<FieldInfo> fields)
            {
                this.Fields = fields as ICollection<FieldInfo> ?? new List<FieldInfo>(fields ?? Enumerable.Empty<FieldInfo>());
            }

            public EmbedInfo()
                : this(null) { }

            public async Task<Embed> BuildAsync(IUser currentUser, IPlaceholdersEngine placeholders, IDiscordClient client, IServiceProvider services, CancellationToken cancellationToken = default)
            {
                EmbedBuilder result = new EmbedBuilder();
                if (!string.IsNullOrEmpty(this.Description))
                    result.WithDescription(await placeholders.ConvertPlaceholdersAsync(this.Description, services, cancellationToken).ConfigureAwait(false));
                if (!string.IsNullOrWhiteSpace(this.Title))
                    result.WithTitle(await placeholders.ConvertPlaceholdersAsync(this.Title, services, cancellationToken).ConfigureAwait(false));
                if (this.Color != null)
                    result.WithColor(this.Color.Value);
                if (!string.IsNullOrWhiteSpace(this.URL))
                    result.WithUrl(this.URL);
                if (!string.IsNullOrWhiteSpace(this.ImageURL))
                    result.WithImageUrl(this.ImageURL);

                foreach (FieldInfo field in this.Fields)
                    result.AddField(field.Name, await placeholders.ConvertPlaceholdersAsync(field.Value, services, cancellationToken).ConfigureAwait(false), field.IsInline);

                if (this.UseCurrentUserAsAuthor)
                    result.WithAuthor(currentUser);
                else if (this.AuthorID != null)
                    result.WithAuthor(await client.GetUserAsync(this.AuthorID.Value, cancellationToken).ConfigureAwait(false));
                else if (this.Author != null)
                    result.WithAuthor(this.Author.Name, this.Author.ImageURL, this.Author.URL);

                if (this.Footer != null)
                    result.WithFooter(await placeholders.ConvertPlaceholdersAsync(this.Footer.Text, services, cancellationToken).ConfigureAwait(false), 
                        this.Footer.UseCurrentUserAsImage ? currentUser.GetSafeAvatarUrl() : this.Footer.ImageURL);

                if (this.UseCurrentUserAsThumbnail)
                    result.WithThumbnailUrl(currentUser.GetMaxAvatarUrl());
                else if (!string.IsNullOrWhiteSpace(this.ThumbnailURL))
                    result.WithThumbnailUrl(this.ThumbnailURL);

                if (this.UseCurrentTimestamp)
                    result.WithCurrentTimestamp();
                else if (this.Timestamp != null)
                    result.WithTimestamp(new DateTimeOffset(this.Timestamp.Value));

                return result.Build();
            }

            public class FieldInfo
            {
                [BsonElement("name"), BsonRequired]
                public string Name { get; set; }
                [BsonElement("value"), BsonRequired]
                public string Value { get; set; }
                [BsonElement("inline"), BsonIgnoreIfDefault, BsonDefaultValue(false)]
                public bool IsInline { get; set; }
            }

            public class UserInfo
            {
                [BsonElement("name"), BsonRequired]
                public string Name { get; set; }
                [BsonElement("url")]
                public string URL { get; set; }
                [BsonElement("image")]
                public string ImageURL { get; set; }
            }

            public class FooterInfo
            {
                [BsonElement("text"), BsonRequired]
                public string Text { get; set; }
                [BsonElement("image")]
                public string ImageURL { get; set; }
                [BsonElement("currentUserImage"), BsonIgnoreIfDefault, BsonDefaultValue(false)]
                public bool UseCurrentUserAsImage { get; set; }
            }
        }
    }
}
