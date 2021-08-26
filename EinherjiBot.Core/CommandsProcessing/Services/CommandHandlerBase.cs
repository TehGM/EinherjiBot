using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    public abstract class CommandHandlerBase : IHostedService, IDisposable
    {
        protected DiscordClient _client { get; }
        protected IOptionsMonitor<CommandsOptions> _commandOptions { get; }
        protected IServiceProvider _serviceProvider { get; }
        protected ILogger _log { get; }
        protected SemaphoreSlim _lock { get; }
        protected CancellationToken _hostCancellationToken { get; private set; }

        public CommandHandlerBase(IServiceProvider serviceProvider, DiscordClient client, IOptionsMonitor<CommandsOptions> commandOptions, ILogger log)
        {
            this._client = client;
            this._commandOptions = commandOptions;
            this._serviceProvider = serviceProvider;
            this._log = log;
            this._lock = new SemaphoreSlim(1, 1);

            _commandOptions.OnChange(async _ => await InitializeCommandsAsync());

            this._client.MessageCreated += HandleCommandInternalAsync;
        }

        protected abstract Task InitializeCommandsAsync();
        protected abstract Task HandleCommandAsync(MessageCreateEventArgs context, int argPos);

        private Task HandleCommandInternalAsync(DiscordClient sender, MessageCreateEventArgs e)
        {
            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            CommandsOptions options = this._commandOptions.CurrentValue;
            // only execute if not a bot message
            if (!options.AcceptBotMessages && e.Message.Author.IsBot)
                return Task.CompletedTask;

            // Don't process the command if it was a system message
            if (e.Message.MessageType != MessageType.Default && e.Message.MessageType == MessageType.Reply)
                return Task.CompletedTask;

            // get prefix and argPos
            bool requirePrefix = e.Guild != null ? options.RequirePublicMessagePrefix : options.RequirePrivateMessagePrefix;
            int argPos = e.Message.GetStringPrefixLength(options.Prefix, StringComparison.OrdinalIgnoreCase);
            if (argPos == -1 && options.AcceptMentionPrefix)
                argPos = e.Message.GetMentionPrefixLength(sender.CurrentUser);

            // if prefix not found but is required, return
            if (requirePrefix && argPos == -1)
                return Task.CompletedTask;

            // Create a WebSocket-based command context based on the message
            return HandleCommandAsync(e, argPos);
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            this._hostCancellationToken = cancellationToken;
            return InitializeCommandsAsync();
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            this.Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this._client.MessageCreated -= HandleCommandInternalAsync;
            this._lock?.Dispose();
        }
    }
}
