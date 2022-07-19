using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    public abstract class CommandHandlerBase : IHostedService, IDisposable
    {
        protected DiscordSocketClient _client { get; }
        protected IOptionsMonitor<CommandsOptions> _commandOptions { get; }
        protected IServiceProvider _serviceProvider { get; }
        protected ILogger _log { get; }
        protected SemaphoreSlim _lock { get; }
        protected CancellationToken _hostCancellationToken { get; private set; }

        public CommandHandlerBase(IServiceProvider serviceProvider, DiscordSocketClient client, IOptionsMonitor<CommandsOptions> commandOptions, ILogger log)
        {
            this._client = client;
            this._commandOptions = commandOptions;
            this._serviceProvider = serviceProvider;
            this._log = log;
            this._lock = new SemaphoreSlim(1, 1);

            _commandOptions.OnChange(async _ => await InitializeCommandsAsync());

            this._client.MessageReceived += HandleCommandInternalAsync;
        }

        protected abstract Task InitializeCommandsAsync();
        protected abstract Task HandleCommandAsync(SocketCommandContext context, int argPos);

        private Task HandleCommandInternalAsync(SocketMessage msg)
        {
            // most of the implementation here taken from https://discord.foxbot.me/docs/guides/commands/intro.html
            // with my own pinch of customizations - TehGM

            // Don't process the command if it was a system message
            if (!(msg is SocketUserMessage message))
                return Task.CompletedTask;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            CommandsOptions options = this._commandOptions.CurrentValue;
            // only execute if not a bot message
            if (!options.AcceptBotMessages && message.Author.IsBot)
                return Task.CompletedTask;
            // get prefix and argPos
            int argPos = 0;
            bool requirePrefix = msg.Channel is SocketGuildChannel ? options.RequirePublicMessagePrefix : options.RequirePrivateMessagePrefix;
            bool hasStringPrefix = message.HasStringPrefix(options.Prefix, ref argPos);
            bool hasMentionPrefix = false;
            if (!hasStringPrefix)
                hasMentionPrefix = message.HasMentionPrefix(_client.CurrentUser, ref argPos);

            // if prefix not found but is required, return
            if (requirePrefix && (!string.IsNullOrWhiteSpace(options.Prefix) && !hasStringPrefix) && (options.AcceptMentionPrefix && !hasMentionPrefix))
                return Task.CompletedTask;

            // Create a WebSocket-based command context based on the message
            SocketCommandContext context = new SocketCommandContext(_client, message);

            return HandleCommandAsync(context, argPos);
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
            this._client.MessageReceived -= HandleCommandInternalAsync;
            this._lock.Dispose();
        }
    }
}
