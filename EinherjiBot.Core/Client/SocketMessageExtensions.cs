using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord.Rest;
using System.Threading;
using System;

namespace TehGM.EinherjiBot
{
    public static class SocketMessageExtensions
    {
        // replying
        public static Task<RestUserMessage> ReplyAsync(this SocketCommandContext message, string text = null, bool isTTS = false, Embed embed = null, AllowedMentions mentions = null, RequestOptions options = null)
            => ReplyAsync(message.Message, text, isTTS, embed, mentions, options);
        public static Task<RestUserMessage> ReplyAsync(this SocketCommandContext message, string text, bool isTTS, Embed embed, AllowedMentions mentions, CancellationToken cancellationToken)
            => ReplyAsync(message.Message, text, isTTS, embed, mentions, new RequestOptions { CancelToken = cancellationToken });
        public static Task<RestUserMessage> ReplyAsync(this SocketCommandContext message, string text, bool isTTS, Embed embed, CancellationToken cancellationToken)
            => ReplyAsync(message.Message, text, isTTS, embed, null, new RequestOptions { CancelToken = cancellationToken });
        public static Task<RestUserMessage> ReplyAsync(this SocketCommandContext message, string text, CancellationToken cancellationToken)
            => ReplyAsync(message.Message, text, false, null, null, cancellationToken);
        public static Task<RestUserMessage> ReplyAsync(this SocketMessage message, string text, bool isTTS, Embed embed, AllowedMentions mentions, CancellationToken cancellationToken)
            => ReplyAsync(message, text, isTTS, embed, mentions, new RequestOptions { CancelToken = cancellationToken });
        public static Task<RestUserMessage> ReplyAsync(this SocketMessage message, string text, bool isTTS, Embed embed, CancellationToken cancellationToken)
            => ReplyAsync(message, text, isTTS, embed, null, new RequestOptions { CancelToken = cancellationToken });
        public static Task<RestUserMessage> ReplyAsync(this SocketMessage message, string text, CancellationToken cancellationToken)
            => ReplyAsync(message, text, false, null, null, cancellationToken );
        public static Task<RestUserMessage> ReplyAsync(this SocketMessage message, string text = null, bool isTTS = false, Embed embed = null, AllowedMentions mentions = null, RequestOptions options = null)
            => message.Channel.SendMessageAsync(text, isTTS, embed, options, mentions);

        // inline replies
        public static Task<RestUserMessage> InlineReplyAsync(this SocketCommandContext message, string text = null, bool isTTS = false, Embed embed = null, AllowedMentions mentions = null, RequestOptions options = null)
            => InlineReplyAsync(message.Message, text, isTTS, embed, mentions, options);
        public static Task<RestUserMessage> InlineReplyAsync(this SocketCommandContext message, string text, bool isTTS, Embed embed, AllowedMentions mentions, CancellationToken cancellationToken)
            => InlineReplyAsync(message.Message, text, isTTS, embed, mentions, new RequestOptions { CancelToken = cancellationToken });
        public static Task<RestUserMessage> InlineReplyAsync(this SocketCommandContext message, string text, bool isTTS, Embed embed, CancellationToken cancellationToken)
            => InlineReplyAsync(message.Message, text, isTTS, embed, null, new RequestOptions { CancelToken = cancellationToken });
        public static Task<RestUserMessage> InlineReplyAsync(this SocketCommandContext message, string text, CancellationToken cancellationToken)
            => InlineReplyAsync(message.Message, text, false, null, null, cancellationToken);
        public static Task<RestUserMessage> InlineReplyAsync(this SocketMessage message, string text, bool isTTS, Embed embed, AllowedMentions mentions, CancellationToken cancellationToken)
            => InlineReplyAsync(message, text, isTTS, embed, mentions, new RequestOptions { CancelToken = cancellationToken });
        public static Task<RestUserMessage> InlineReplyAsync(this SocketMessage message, string text, bool isTTS, Embed embed, CancellationToken cancellationToken)
            => InlineReplyAsync(message, text, isTTS, embed, null, new RequestOptions { CancelToken = cancellationToken });
        public static Task<RestUserMessage> InlineReplyAsync(this SocketMessage message, string text, CancellationToken cancellationToken)
            => InlineReplyAsync(message, text, false, null, null, cancellationToken);
        public static Task<RestUserMessage> InlineReplyAsync(this SocketMessage message, string text = null, bool isTTS = false, Embed embed = null, AllowedMentions mentions = null, RequestOptions options = null)
            => message.Channel.SendMessageAsync(text, isTTS, embed, options, mentions, new MessageReference(message.Id, message.Channel?.Id, (message.Channel as IGuildChannel)?.GuildId));

        // modifying
        public static Task ModifyAsync(this IUserMessage message, Action<MessageProperties> func, CancellationToken cancellationToken)
            => message.ModifyAsync(func, new RequestOptions { CancelToken = cancellationToken });

        // deleting
        public static Task DeleteAsync(this IDeletable entity, CancellationToken cancellationToken)
            => entity.DeleteAsync(new RequestOptions { CancelToken = cancellationToken });
    }
}
