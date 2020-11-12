using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace TehGM.EinherjiBot
{
    public static class SocketmessageChannelExtensions
    {
        public static Task<RestUserMessage> SendMessageAsync(this ISocketMessageChannel channel, string text, bool isTTS, Embed embed, CancellationToken cancellationToken)
            => SendMessageAsync(channel, text, isTTS, embed, new RequestOptions { CancelToken = cancellationToken });

        public static Task<RestUserMessage> SendMessageAsync(this ISocketMessageChannel channel, string text, CancellationToken cancellationToken)
            => SendMessageAsync(channel, text, false, null, cancellationToken);

        // base method
        public static Task<RestUserMessage> SendMessageAsync(this ISocketMessageChannel channel, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => channel.SendMessageAsync(text, isTTS, embed, options);
    }
}
