using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing.Checks;

namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    /// <summary>Handler that allows use of DSharpPlus' default commands system.</summary>
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

        protected override async Task HandleCommandAsync(MessageCreateEventArgs e, int argPos)
        {
            CommandsNextExtension commandsNext = base._client.GetCommandsNext();

            string prefix = string.Empty;
            string content = e.Message.Content;
            if (argPos > 0)
            {
                prefix = e.Message.Content.Remove(0, argPos);
                content = e.Message.Content.Substring(argPos);
            }

            Command command = commandsNext.FindCommand(content, out string args);
            if (command == null) 
                return;

            CommandDescriptor descriptor = new CommandDescriptor(command);
            CommandContext context = new CommandContext(descriptor, e, base._client);
            foreach (CommandCheckAttribute check in descriptor.CommandChecks)
            {
                CommandCheckResult result = await check.RunCheckAsync(context, base._serviceProvider, base._hostCancellationToken).ConfigureAwait(false);
                if (result.ResultType != CommandCheckResultType.Continue)
                {
                    base.LogCommandCheck(result, command.QualifiedName);
                    return;
                }
            }

            DSharpPlus.CommandsNext.CommandContext ctx = commandsNext.CreateContext(e.Message, prefix, command, args);
            await commandsNext.ExecuteCommandAsync(ctx).ConfigureAwait(false);
        }
    }
}
