using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing;

namespace TehGM.EinherjiBot.RandomStatus
{
    [LoadRegexCommands]
    [PersistentModule(PreInitialize = true, SharedInstance = true)]
    public class RandomStatusHandler : IDisposable
    {
        private readonly IOptionsMonitor<RandomStatusOptions> _options;
        private readonly IOptionsMonitor<EinherjiOptions> _einherjiOptions;
        private readonly DiscordSocketClient _client;
        private readonly ILogger _log;
        private readonly IDisposable _optionsChangeRegistration;
        private readonly Random _random;
        private readonly IAdvancedStatusConverter _converter;
        private CancellationTokenSource _cts;

        private DateTime _lastChangeUtc;

        public RandomStatusHandler(IOptionsMonitor<EinherjiOptions> einherjiOptions, IOptionsMonitor<RandomStatusOptions> options, 
            DiscordSocketClient client, ILogger<RandomStatusHandler> log, IAdvancedStatusConverter converter)
        {
            this._options = options;
            this._einherjiOptions = einherjiOptions;
            this._client = client;
            this._log = log;
            this._random = new Random();
            this._converter = converter;

            this._optionsChangeRegistration = this._options.OnChange(options =>
            {
                if (options.IsEnabled)
                    StartBackgroundLoop();
            });

            this._client.Ready += OnClientReady;
        }

        private Task OnClientReady()
        {
            StartBackgroundLoop();
            return Task.CompletedTask;
        }

        private async Task AutoChangeLoopAsync(CancellationToken cancellationToken)
        {
            this._log.LogDebug("Starting status randomization loop. Change rate is {ChangeRate}", this._options.CurrentValue.ChangeRate);
            if (this._options.CurrentValue.ChangeRate <= TimeSpan.FromSeconds(10))
                this._log.LogWarning("Change rate is less than 10 seconds!");
            while (!cancellationToken.IsCancellationRequested && this._options.CurrentValue.IsEnabled)
            {
                DateTime nextChangeUtc = this._lastChangeUtc + this._options.CurrentValue.ChangeRate;
                TimeSpan remainingWait = nextChangeUtc - DateTime.UtcNow;
                if (remainingWait > TimeSpan.Zero)
                    await Task.Delay(remainingWait, cancellationToken).ConfigureAwait(false);
                await RandomizeStatusAsync(this._client, cancellationToken).ConfigureAwait(false);
                this._log.LogTrace("Next status change: {ChangeTime}", this._lastChangeUtc + this._options.CurrentValue.ChangeRate);
            }
        }

        [RegexCommand("^randomize status")]
        [Hidden]
        private async Task CmdRandomizeStatus(SocketCommandContext context, CancellationToken cancellationToken = default)
        {
            EinherjiOptions einherjiOptions = this._einherjiOptions.CurrentValue;
            if (context.User.Id != einherjiOptions.AuthorID)
            {
                await context.ReplyAsync($"{einherjiOptions.FailureSymbol} You have no rights to tell me ~~how to live my life~~ do this!",
                    cancellationToken).ConfigureAwait(false);
                return;
            }

            Status status = await RandomizeStatusAsync(context.Client, cancellationToken).ConfigureAwait(false);
            if (status != null)
            {
                await context.ReplyAsync($"{einherjiOptions.SuccessSymbol} Status changed: `{status.Text.Replace("`", "\\`")}`", cancellationToken).ConfigureAwait(false);
                // restart loop
                StartBackgroundLoop();
            }
            else if (this._options.CurrentValue.Statuses?.Any() != true)
                await context.ReplyAsync($"{einherjiOptions.FailureSymbol} Status not changed - ensure status list is not empty.", cancellationToken).ConfigureAwait(false);
        }

        private async Task<Status> RandomizeStatusAsync(DiscordSocketClient client, CancellationToken cancellationToken)
        {
            Status status = await this.PickStatusAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (client.ConnectionState != Discord.ConnectionState.Connected)
                    return null;
                if (status == null)
                    return null;
                if (!string.IsNullOrWhiteSpace(status.Text))
                    this._log.LogDebug("Changing status to `{NewStatus}`", status);
                else
                    this._log.LogDebug("Clearing status");
                await client.SetGameAsync(status.Text, status.Link, status.ActivityType);
                return status;
            }
            catch (Exception ex) when (this._options.CurrentValue.IsEnabled && ex.LogAsError(this._log, "Failed changing status to {Status}", status))
            {
                return null;
            }
            finally
            {
                this._lastChangeUtc = DateTime.UtcNow;
            }
        }

        private async Task<Status> PickStatusAsync(CancellationToken cancellationToken)
        {
            RandomStatusOptions options = this._options.CurrentValue;
            if (options.Statuses?.Any() != true)
                return null;
            int statusIndex = this._random.Next(options.Statuses.Length);
            Status status = options.Statuses[statusIndex];
            if (!status.IsAdvanced)
                return status;

            string text = await this._converter.ConvertAsync(status, cancellationToken).ConfigureAwait(false);
            Status result = new Status();
            result.ActivityType = status.ActivityType;
            result.IsAdvanced = status.IsAdvanced;
            result.Link = status.Link;
            result.Text = text;
            return result;
        }

        private void CancelBackgroundLoop()
        {
            try { this._cts?.Cancel(); } catch { }
            try { this._cts?.Dispose(); } catch { }
            this._cts = null;
        }

        private void StartBackgroundLoop()
        {
            if (!this._options.CurrentValue.IsEnabled)
                return;
            CancelBackgroundLoop();
            this._cts = new CancellationTokenSource();
            _ = AutoChangeLoopAsync(_cts.Token);
        }

        public void Dispose()
        {
            try { this._optionsChangeRegistration?.Dispose(); } catch { }
            try { this._client.Ready -= OnClientReady; } catch { }
            CancelBackgroundLoop();
        }
    }
}
