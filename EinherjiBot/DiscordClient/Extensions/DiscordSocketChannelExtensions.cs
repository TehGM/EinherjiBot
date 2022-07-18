using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace TehGM.EinherjiBot
{
    public static class DiscordSocketChannelExtensions
    {
        // SendMessageAsync
        public static Task<RestUserMessage> SendMessageAsync(this ISocketMessageChannel channel, string text, bool isTTS, Embed embed, CancellationToken cancellationToken)
            => SendMessageAsync(channel, text, isTTS, embed, cancellationToken.ToRequestOptions());
        public static Task<RestUserMessage> SendMessageAsync(this ISocketMessageChannel channel, string text, CancellationToken cancellationToken)
            => SendMessageAsync(channel, text, false, null, cancellationToken);
        public static Task<RestUserMessage> SendMessageAsync(this ISocketMessageChannel channel, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => channel.SendMessageAsync(text, isTTS, embed, options);

        // DeleteMessageAsync / DeleteMessagesAsync
        public static Task DeleteMessagesAsync(this ITextChannel channel, IEnumerable<IMessage> messages, CancellationToken cancellationToken)
            => channel.DeleteMessagesAsync(messages, cancellationToken.ToRequestOptions());
        public static Task DeleteMessageAsync(this IMessageChannel channel, IMessage message, CancellationToken cancellationToken)
            => channel.DeleteMessageAsync(message, cancellationToken.ToRequestOptions());

        // GetMessagesAsync
        public static IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(this IMessageChannel channel, int limit, CancellationToken cancellationToken)
            => channel.GetMessagesAsync(limit, CacheMode.AllowDownload, cancellationToken.ToRequestOptions());
        public static IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(this IMessageChannel channel, IMessage fromMessage, Direction dir, int limit, CancellationToken cancellationToken)
            => channel.GetMessagesAsync(fromMessage, dir, limit, cancellationToken);
        public static IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(this IMessageChannel channel, ulong fromMessageId, Direction dir, int limit, CancellationToken cancellationToken)
            => channel.GetMessagesAsync(fromMessageId, dir, limit, cancellationToken);
    }
}
