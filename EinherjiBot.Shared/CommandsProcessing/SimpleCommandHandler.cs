using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    /// <summary>Handler that allows use of Discord.NET's default commands system.</summary>
    public class SimpleCommandHandler : IHostedService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private CommandService _commands;
        private readonly IOptionsMonitor<CommandOptions> _commandOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _log;

        public SimpleCommandHandler(IServiceProvider serviceProvider, DiscordSocketClient client, IOptionsMonitor<CommandOptions> commandOptions, ILogger<SimpleCommandHandler> log)
        {
            this._client = client;
            this._commandOptions = commandOptions;
            this._serviceProvider = serviceProvider;
            this._log = log;

            _commandOptions.OnChange(async _ => await InitializeCommandServiceAsync());

            this._client.MessageReceived += HandleCommandAsync;
        }

        private async Task InitializeCommandServiceAsync()
        {
            _log.LogDebug("Initializing CommandService");

            try { (_commands as IDisposable)?.Dispose(); } catch { }

            CommandOptions options = this._commandOptions.CurrentValue;
            CommandServiceConfig config = new CommandServiceConfig();
            config.CaseSensitiveCommands = options.CaseSensitive;
            if (options.DefaultRunMode != RunMode.Default)
                config.DefaultRunMode = options.DefaultRunMode;
            config.IgnoreExtraArgs = options.IgnoreExtraArgs;
            this._commands = new CommandService(config);
            foreach (Assembly asm in options.Assemblies)
                await this._commands.AddModulesAsync(asm, _serviceProvider).ConfigureAwait(false);
            foreach (Type t in options.Classes)
                await this._commands.AddModuleAsync(t, _serviceProvider).ConfigureAwait(false);
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            // most of the implementation here taken from https://discord.foxbot.me/docs/guides/commands/intro.html
            // with my own pinch of customizations - TehGM

            // Don't process the command if it was a system message
            if (!(msg is SocketUserMessage message))
                return;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            CommandOptions options = this._commandOptions.CurrentValue;
            // only execute if not a bot message
            if (!options.AcceptBotMessages && message.Author.IsBot)
                return;
            // get prefix and argPos
            int argPos = 0;
            bool requirePrefix = msg.Channel is SocketGuildChannel ? options.RequirePublicMessagePrefix : options.RequirePrivateMessagePrefix;
            bool hasStringPrefix = message.HasStringPrefix(options.Prefix, ref argPos);
            bool hasMentionPrefix = false;
            if (!hasStringPrefix)
                hasMentionPrefix = message.HasMentionPrefix(_client.CurrentUser, ref argPos);

            // if prefix not found but is required, return
            if (requirePrefix && (!string.IsNullOrWhiteSpace(options.Prefix) && !hasStringPrefix) && (options.AcceptMentionPrefix && !hasMentionPrefix))
                return;

            // Create a WebSocket-based command context based on the message
            SocketCommandContext context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.

            // Keep in mind that result does not indicate a return value
            // rather an object stating if the command executed successfully.
            IResult result = await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _serviceProvider)
                .ConfigureAwait(false);
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
            => InitializeCommandServiceAsync();

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this._client.MessageReceived -= HandleCommandAsync;
        }
    }
}
