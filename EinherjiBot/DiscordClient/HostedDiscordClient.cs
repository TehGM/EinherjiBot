using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace TehGM.EinherjiBot.DiscordClient
{
    public class HostedDiscordClient : IHostedDiscordClient, IHostedService, IDisposable
    {
        public IDiscordClient Client => _client;

        private readonly ILogger _log;
        private readonly IOptionsMonitor<DiscordOptions> _discordOptions;
        private readonly IDisposable _optionsChangeHandle;
        private DiscordSocketClient _client;
        private bool _started = false;

        public HostedDiscordClient(IOptionsMonitor<DiscordOptions> discordOptions, ILogger<HostedDiscordClient> log)
        {
            this._discordOptions = discordOptions;
            this._log = log;

            DiscordSocketConfig clientConfig = new DiscordSocketConfig();
            clientConfig.LogLevel = LogSeverity.Verbose;
            clientConfig.LogGatewayIntentWarnings = false;
            this._client = new DiscordSocketClient(clientConfig);
            this._client.Log += this.OnClientLog;

            this._optionsChangeHandle = this._discordOptions.OnChange(async _ =>
            {
                if (this.Client.ConnectionState == ConnectionState.Connected || this.Client.ConnectionState == ConnectionState.Connecting)
                {
                    this._log.LogInformation("Options changed, reconnecting client");
                    await this.StopClientAsync().ConfigureAwait(false);
                    await this.StartClientAsync().ConfigureAwait(false);
                }
            });
        }

        public async Task StartClientAsync()
        {
            await this._client.LoginAsync(TokenType.Bot, this._discordOptions.CurrentValue.BotToken).ConfigureAwait(false);
            await this._client.StartAsync().ConfigureAwait(false);
        }

        public async Task StopClientAsync()
        {
            if (this._client.LoginState == LoginState.LoggedIn || this._client.LoginState == LoginState.LoggingIn)
                await this._client.LogoutAsync().ConfigureAwait(false);
            if (this._client.ConnectionState == ConnectionState.Connected || this._client.ConnectionState == ConnectionState.Connecting)
                await this._client.StopAsync().ConfigureAwait(false);
        }

        private Task OnClientLog(LogMessage message)
        {
            this._log.Log(message);
            return Task.CompletedTask;
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            if (this._started)
                return Task.CompletedTask;

            this._started = true;
            return this.StartClientAsync();
        }

        async Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            await this.StopClientAsync().ConfigureAwait(false);
            this.Dispose();
        }

        public static implicit operator DiscordSocketClient(HostedDiscordClient client)
            => client._client;

        public void Dispose()
        {
            try { this._client.Log -= this.OnClientLog; } catch { }
            try { this._client?.Dispose(); } catch { }
            try { this._optionsChangeHandle?.Dispose(); } catch { }
        }
    }
}
