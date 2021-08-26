using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TehGM.EinherjiBot.Client
{
    public class HostedDiscordClient : IHostedDiscordClient, IHostedService, IDisposable
    {
        public DiscordClient Client { get; private set; }

        private readonly ILogger _log;
        private readonly ILoggerFactory _logFactory;
        private readonly IOptionsMonitor<DiscordOptions> _discordOptions;
        private bool _started = false;

        public HostedDiscordClient(IOptionsMonitor<DiscordOptions> discordOptions, ILogger<HostedDiscordClient> log, ILoggerFactory logFactory)
        {
            this._discordOptions = discordOptions;
            this._log = log;
            this._logFactory = logFactory;

            this._log.LogDebug("Creating Discord client");
            DiscordConfiguration clientConfig = new DiscordConfiguration();
            clientConfig.AutoReconnect = this._discordOptions.CurrentValue.AutoConnectGateway;
            clientConfig.Intents = DiscordIntents.All | DiscordIntents.AllUnprivileged;
            clientConfig.LoggerFactory = this._logFactory;
            clientConfig.MessageCacheSize = 512;
            clientConfig.Token = this._discordOptions.CurrentValue.BotToken;
            clientConfig.TokenType = TokenType.Bot;
            this.Client = new DiscordClient(clientConfig);
        }

        public Task StartClientAsync()
            => this.Client.ConnectAsync();

        public Task StopClientAsync()
            => this.Client.DisconnectAsync();

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            if (_started)
                return Task.CompletedTask;

            _started = true;
            if (_discordOptions.CurrentValue.AutoConnectGateway)
                return StartClientAsync();
            else
                return Task.CompletedTask;
        }

        async Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            await StopClientAsync().ConfigureAwait(false);
            Dispose();
        }

        public static implicit operator DiscordClient(HostedDiscordClient client)
            => client.Client;

        public void Dispose()
        {
            try { this.Client?.Dispose(); } catch { }
            this.Client = null;
        }
    }
}
