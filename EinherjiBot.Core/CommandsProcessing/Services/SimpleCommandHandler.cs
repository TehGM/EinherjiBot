using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    /// <summary>Handler that allows use of Discord.NET's default commands system.</summary>
    public class SimpleCommandHandler : CommandHandlerBase
    {
        public CommandService Commands { get; private set; }

        public SimpleCommandHandler(IServiceProvider serviceProvider, DiscordSocketClient client, IOptionsMonitor<CommandsOptions> commandOptions, ILogger<SimpleCommandHandler> log)
            : base(serviceProvider, client, commandOptions, log) { }

        protected override async Task InitializeCommandsAsync()
        {
            _log.LogDebug("Initializing CommandService");

            try { (Commands as IDisposable)?.Dispose(); } catch { }

            CommandsOptions options = this._commandOptions.CurrentValue;
            CommandServiceConfig config = new CommandServiceConfig();
            config.CaseSensitiveCommands = options.CaseSensitive;
            if (options.DefaultRunMode != RunMode.Default)
                config.DefaultRunMode = options.DefaultRunMode;
            config.IgnoreExtraArgs = options.IgnoreExtraArgs;
            this.Commands = new CommandService(config);
            foreach (Assembly asm in options.Assemblies)
                await this.Commands.AddModulesAsync(asm, _serviceProvider).ConfigureAwait(false);
            foreach (Type t in options.Classes)
                await this.Commands.AddModuleAsync(t, _serviceProvider).ConfigureAwait(false);
        }

        protected override async Task HandleCommandAsync(SocketCommandContext context, int argPos)
        {
            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.

            // Keep in mind that result does not indicate a return value
            // rather an object stating if the command executed successfully.
            IResult result = await Commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _serviceProvider)
                .ConfigureAwait(false);
            if (!result.IsSuccess && result is ExecuteResult executeResult && executeResult.Exception != null && !(executeResult.Exception is OperationCanceledException))
                _log.LogError(executeResult.Exception, "Unhandled Exception when executing a basic command");
        }
    }
}
