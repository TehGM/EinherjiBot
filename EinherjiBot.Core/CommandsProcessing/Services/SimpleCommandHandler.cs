using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    /// <summary>Handler that allows use of Discord.NET's default commands system.</summary>
    public class SimpleCommandHandler : CommandHandlerBase
    {
        public IEnumerable<Command> Commands => base._client.GetCommandsNext().RegisteredCommands.Values;

        public SimpleCommandHandler(IServiceProvider serviceProvider, DiscordClient client, IOptionsMonitor<CommandsOptions> commandOptions, ILogger<SimpleCommandHandler> log)
            : base(serviceProvider, client, commandOptions, log) { }

        protected override Task InitializeCommandsAsync()
        {
            _log.LogDebug("Initializing SimpleCommandHandler");
            CommandsOptions options = base._commandOptions.CurrentValue;

            base._client.UseCommandsNext(new CommandsNextConfiguration()
            {
                UseDefaultCommandHandler = false,
                Services = base._serviceProvider,
                EnableDms = true,
                CaseSensitive = options.CaseSensitive,
                EnableDefaultHelp = false,
                DmHelp = false,
                IgnoreExtraArguments = true,
                EnableMentionPrefix = options.AcceptMentionPrefix,
                StringPrefixes = new string[] { options.Prefix }
            });
            return Task.CompletedTask;
        }

        protected override Task HandleCommandAsync(MessageCreateEventArgs context, int argPos)
        {
            CommandsNextExtension commandsNext = base._client.GetCommandsNext();

            string prefix = context.Message.Content.Remove(0, argPos);
            string content = context.Message.Content.Substring(argPos);

            Command command = commandsNext.FindCommand(content, out string args);
            if (command == null) 
                return Task.CompletedTask;
            CommandContext ctx = commandsNext.CreateContext(context.Message, prefix, command, args);
            return commandsNext.ExecuteCommandAsync(ctx);
        }
    }
}
