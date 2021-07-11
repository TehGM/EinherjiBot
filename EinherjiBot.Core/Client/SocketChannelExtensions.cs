using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace TehGM.EinherjiBot
{
    public static class SocketChannelExtensions
    {
        // SendMessageAsync
        public static Task<RestUserMessage> SendMessageAsync(this ISocketMessageChannel channel, string text, bool isTTS, Embed embed, CancellationToken cancellationToken)
            => SendMessageAsync(channel, text, isTTS, embed, new RequestOptions { CancelToken = cancellationToken });
        public static Task<RestUserMessage> SendMessageAsync(this ISocketMessageChannel channel, string text, CancellationToken cancellationToken)
            => SendMessageAsync(channel, text, false, null, cancellationToken);
        public static Task<RestUserMessage> SendMessageAsync(this ISocketMessageChannel channel, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => channel.SendMessageAsync(text, isTTS, embed, options);

        // DeleteMessageAsync / DeleteMessagesAsync
        public static Task DeleteMessagesAsync(this ITextChannel channel, IEnumerable<IMessage> messages, CancellationToken cancellationToken)
            => channel.DeleteMessagesAsync(messages, new RequestOptions { CancelToken = cancellationToken });
        public static Task DeleteMessageAsync(this IMessageChannel channel, IMessage message, CancellationToken cancellationToken)
            => channel.DeleteMessageAsync(message, new RequestOptions { CancelToken = cancellationToken });

        // GetMessagesAsync
        public static IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(this SocketTextChannel channel, int limit, CancellationToken cancellationToken)
            => channel.GetMessagesAsync(limit, new RequestOptions { CancelToken = cancellationToken });
        public static IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(this SocketTextChannel channel, IMessage fromMessage, Direction dir, int limit, CancellationToken cancellationToken)
            => channel.GetMessagesAsync(fromMessage, dir, limit, new RequestOptions { CancelToken = cancellationToken });
        public static IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(this SocketTextChannel channel, ulong fromMessageId, Direction dir, int limit, CancellationToken cancellationToken)
            => channel.GetMessagesAsync(fromMessageId, dir, limit, new RequestOptions { CancelToken = cancellationToken });
    }
}
