using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;

namespace TehGM.EinherjiBot
{
    public static class DiscordSocketMessageExtensions
    {
        // replying
        public static Task<IUserMessage> ReplyAsync(this SocketCommandContext message, string text = null, bool isTTS = false, Embed embed = null, AllowedMentions mentions = null, RequestOptions options = null)
            => ReplyAsync(message.Message, text, isTTS, embed, mentions, options);
        public static Task<IUserMessage> ReplyAsync(this SocketCommandContext message, string text, bool isTTS, Embed embed, AllowedMentions mentions, CancellationToken cancellationToken)
            => ReplyAsync(message.Message, text, isTTS, embed, mentions, cancellationToken.ToRequestOptions());
        public static Task<IUserMessage> ReplyAsync(this SocketCommandContext message, string text, bool isTTS, Embed embed, CancellationToken cancellationToken)
            => ReplyAsync(message.Message, text, isTTS, embed, null, cancellationToken.ToRequestOptions());
        public static Task<IUserMessage> ReplyAsync(this SocketCommandContext message, string text, CancellationToken cancellationToken)
            => ReplyAsync(message.Message, text, false, null, null, cancellationToken);
        public static Task<IUserMessage> ReplyAsync(this IUserMessage message, string text, bool isTTS, Embed embed, AllowedMentions mentions, CancellationToken cancellationToken)
            => ReplyAsync(message, text, isTTS, embed, mentions, cancellationToken.ToRequestOptions());
        public static Task<IUserMessage> ReplyAsync(this IUserMessage message, string text, bool isTTS, Embed embed, CancellationToken cancellationToken)
            => ReplyAsync(message, text, isTTS, embed, null, cancellationToken.ToRequestOptions());
        public static Task<IUserMessage> ReplyAsync(this IUserMessage message, string text, CancellationToken cancellationToken)
            => ReplyAsync(message, text, false, null, null, cancellationToken);
        public static Task<IUserMessage> ReplyAsync(this IUserMessage message, string text = null, bool isTTS = false, Embed embed = null, AllowedMentions mentions = null, RequestOptions options = null)
            => message.Channel.SendMessageAsync(text, isTTS, embed, options, mentions);

        // inline replies
        public static Task<IUserMessage> InlineReplyAsync(this SocketCommandContext message, string text = null, bool isTTS = false, Embed embed = null, AllowedMentions mentions = null, RequestOptions options = null)
            => InlineReplyAsync(message.Message, text, isTTS, embed, mentions, options);
        public static Task<IUserMessage> InlineReplyAsync(this SocketCommandContext message, string text, bool isTTS, Embed embed, AllowedMentions mentions, CancellationToken cancellationToken)
            => InlineReplyAsync(message.Message, text, isTTS, embed, mentions, cancellationToken.ToRequestOptions());
        public static Task<IUserMessage> InlineReplyAsync(this SocketCommandContext message, string text, bool isTTS, Embed embed, CancellationToken cancellationToken)
            => InlineReplyAsync(message.Message, text, isTTS, embed, null, cancellationToken.ToRequestOptions());
        public static Task<IUserMessage> InlineReplyAsync(this SocketCommandContext message, string text, CancellationToken cancellationToken)
            => InlineReplyAsync(message.Message, text, false, null, null, cancellationToken);
        public static Task<IUserMessage> InlineReplyAsync(this IUserMessage message, string text, bool isTTS, Embed embed, AllowedMentions mentions, CancellationToken cancellationToken)
            => InlineReplyAsync(message, text, isTTS, embed, mentions, cancellationToken.ToRequestOptions());
        public static Task<IUserMessage> InlineReplyAsync(this IUserMessage message, string text, bool isTTS, Embed embed, CancellationToken cancellationToken)
            => InlineReplyAsync(message, text, isTTS, embed, null, cancellationToken.ToRequestOptions());
        public static Task<IUserMessage> InlineReplyAsync(this IUserMessage message, string text, CancellationToken cancellationToken)
            => InlineReplyAsync(message, text, false, null, null, cancellationToken);
        public static Task<IUserMessage> InlineReplyAsync(this IUserMessage message, string text = null, bool isTTS = false, Embed embed = null, AllowedMentions mentions = null, RequestOptions options = null)
            => message.Channel.SendMessageAsync(text, isTTS, embed, options, mentions, new MessageReference(message.Id, message.Channel?.Id, (message.Channel as IGuildChannel)?.GuildId));

        // modifying
        public static Task ModifyAsync(this IUserMessage message, Action<MessageProperties> func, CancellationToken cancellationToken)
            => message.ModifyAsync(func, cancellationToken.ToRequestOptions());

        // deleting
        public static Task DeleteAsync(this IDeletable entity, CancellationToken cancellationToken)
            => entity.DeleteAsync(cancellationToken.ToRequestOptions());
    }
}
