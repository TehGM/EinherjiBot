using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    public class RegexUserCommand : ICommandProcessor
    {
        public const RegexOptions DefaultRegexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline;
        public static ICommandVerificator DefaultVerificator => CommandVerificator.DefaultPrefixed;

        private readonly Regex _regex;
        private readonly Func<SocketCommandContext, Match, Task> _method;
        private readonly ICommandVerificator _verificator;

        public RegexUserCommand(ICommandVerificator verificator, Regex regex, Func<SocketCommandContext, Match, Task> method)
        {
            this._verificator = verificator;
            this._regex = regex;
            this._method = method;
        }
        public RegexUserCommand(Regex regex, Func<SocketCommandContext, Match, Task> method)
            : this(DefaultVerificator, regex, method) { }
        public RegexUserCommand(ICommandVerificator verificator, string pattern, RegexOptions options, Func<SocketCommandContext, Match, Task> method)
            : this(verificator, new Regex(pattern, options), method) { }
        public RegexUserCommand(string pattern, RegexOptions options, Func<SocketCommandContext, Match, Task> method)
            : this(DefaultVerificator, new Regex(pattern, options), method) { }

        public async Task<bool> ProcessAsync(DiscordSocketClient client, SocketMessage message)
        {
            if (!(message is SocketUserMessage msg))
                return false;
            SocketCommandContext ctx = new SocketCommandContext(client, msg);
            if (!_verificator.Verify(ctx, out string cmd))
                return false;

            Match match = _regex.Match(cmd);
            if (match == null || !match.Success)
                return false;

            await _method.Invoke(ctx, match);
            return true;
        }
    }
}
