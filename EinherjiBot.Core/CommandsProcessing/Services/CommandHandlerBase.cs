using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing.Checks;

namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    public abstract class CommandHandlerBase : IHostedService, IDisposable
    {
        protected DiscordClient Client { get; }
        protected IOptionsMonitor<CommandsOptions> CommandOptions { get; }
        protected IServiceProvider ServiceProvider { get; }
        protected ILogger Log { get; }
        protected SemaphoreSlim Lock { get; }
        protected CancellationToken HostCancellationToken { get; private set; }

        public CommandHandlerBase(IServiceProvider serviceProvider, DiscordClient client, IOptionsMonitor<CommandsOptions> commandOptions, ILogger log)
        {
            this.Client = client;
            this.CommandOptions = commandOptions;
            this.ServiceProvider = serviceProvider;
            this.Log = log;
            this.Lock = new SemaphoreSlim(1, 1);

            CommandOptions.OnChange(async _ => await InitializeCommandsAsync());

            this.Client.MessageCreated += HandleCommandInternalAsync;
        }

        protected abstract Task InitializeCommandsAsync();
        protected abstract Task HandleCommandAsync(MessageCreateEventArgs context, int argPos);

        private Task HandleCommandInternalAsync(DiscordClient sender, MessageCreateEventArgs e)
        {
            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            CommandsOptions options = this.CommandOptions.CurrentValue;
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
            this.HostCancellationToken = cancellationToken;
            return InitializeCommandsAsync();
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            this.Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.Client.MessageCreated -= HandleCommandInternalAsync;
            this.Lock?.Dispose();
        }

        protected void LogCommandCheck(CommandCheckResult result, string commandName)
        {
            string resultText = string.Empty;
            if (result.ResultType == CommandCheckResultType.Skip)
                resultText = " skipping";
            else if (result.ResultType == CommandCheckResultType.Abort)
                resultText = " aborting";
            string message = $"Command {{CommandName}}{resultText}";
            if (!string.IsNullOrWhiteSpace(result.Message))
                message += $": {result.Message}";
            this.Log.Log(result.Error != null ? LogLevel.Error : LogLevel.Trace, result.Error, message, commandName);
        }
    }
}
