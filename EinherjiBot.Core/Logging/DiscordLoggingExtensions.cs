using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using TehGM.EinherjiBot.CommandsProcessing;

namespace TehGM.EinherjiBot
{
    public static class DiscordLoggingExtensions
    {
        public static IDisposable UseSource(string source)
            => LogContext.PushProperty("Source", source);
        public static IDisposable UseSource(this ILogger log, string source)
            => LogContext.PushProperty("Source", source);

        public static IDisposable BeginCommandScope(this ILogger log, CommandContext context, object handler = null, [CallerMemberName] string cmdName = null)
            => BeginCommandScope(log, context, handler?.GetType(), cmdName);
        public static IDisposable BeginCommandScope(this ILogger log, CommandContext context, Type handlerType = null, [CallerMemberName] string cmdName = null)
        {
            Dictionary<string, object> state = new Dictionary<string, object>
            {
                { "Command.UserID", context.User?.Id },
                { "Command.MessageID", context.Message?.Id },
                { "Command.ChannelID", context.Channel?.Id },
                { "Command.GuildID", context.Guild?.Id }
            };
            if (!string.IsNullOrWhiteSpace(context.Descriptor.DisplayName))
                state.Add("Command.DisplayName", context.Descriptor.DisplayName);
            if (!string.IsNullOrWhiteSpace(cmdName))
                state.Add("Command.Method", cmdName);
            if (handlerType != null)
                state.Add("Command.Handler", handlerType.Name);
            return log.BeginScope(state);
        }

        public static IDisposable BeginCommandScope(this ILogger log, MessageCreateEventArgs context, object handler = null, [CallerMemberName] string cmdName = null)
            => BeginCommandScope(log, context, handler?.GetType(), cmdName);

        public static IDisposable BeginCommandScope(this ILogger log, MessageCreateEventArgs context, Type handlerType = null, [CallerMemberName] string cmdName = null)
        {
            Dictionary<string, object> state = new Dictionary<string, object>
            {
                { "Command.UserID", context.Author?.Id },
                { "Command.MessageID", context.Message?.Id },
                { "Command.ChannelID", context.Channel?.Id },
                { "Command.GuildID", context.Guild?.Id }
            };
            if (!string.IsNullOrWhiteSpace(cmdName))
                state.Add("Command.Method", cmdName);
            if (handlerType != null)
                state.Add("Command.Handler", handlerType.Name);
            return log.BeginScope(state);
        }
    }
}
