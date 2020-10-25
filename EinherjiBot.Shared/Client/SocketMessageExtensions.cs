using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord.Rest;
using System.Threading;

namespace TehGM.EinherjiBot
{
    public static class SocketMessageExtensions
    {
        public static Task<RestUserMessage> ReplyAsync(this SocketCommandContext message, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => ReplyAsync(message.Message, text, isTTS, embed, options);

        public static Task<RestUserMessage> ReplyAsync(this SocketCommandContext message, string text, bool isTTS, Embed embed, CancellationToken cancellationToken)
            => ReplyAsync(message.Message, text, isTTS, embed, new RequestOptions { CancelToken = cancellationToken });

        public static Task<RestUserMessage> ReplyAsync(this SocketCommandContext message, string text, CancellationToken cancellationToken)
            => ReplyAsync(message.Message, text, false, null, cancellationToken);

        public static Task<RestUserMessage> ReplyAsync(this SocketMessage message, string text, bool isTTS, Embed embed, CancellationToken cancellationToken)
            => ReplyAsync(message, text, isTTS, embed, new RequestOptions { CancelToken = cancellationToken });

        public static Task<RestUserMessage> ReplyAsync(this SocketMessage message, string text, CancellationToken cancellationToken)
            => ReplyAsync(message, text, false, null, cancellationToken );


        // base method
        public static Task<RestUserMessage> ReplyAsync(this SocketMessage message, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => message.Channel.SendMessageAsync(text, isTTS, embed, options);
    }
}
