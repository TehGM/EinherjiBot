using Discord;
using Discord.WebSocket;
using TehGM.EinherjiBot.PlaceholdersEngine;
using TehGM.Utilities.Randomization;

namespace TehGM.EinherjiBot.RandomStatus.Services
{
    /// <summary>Background service that periodically scans blog channels for last activity and activates or deactivates them.</summary>
    internal class RandomStatusService : AutostartService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IRandomizer _randomizer;
        private readonly IPlaceholdersEngine _placeholders;
        private readonly IStatusProvider _provider;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<RandomStatusOptions> _options;

        private DateTime _lastChangeUtc;

        public RandomStatusService(DiscordSocketClient client, IRandomizer randomizer, IPlaceholdersEngine placeholders, IStatusProvider provider,
            ILogger<RandomStatusService> log, IOptionsMonitor<RandomStatusOptions> options)
        {
            this._client = client;
            this._randomizer = randomizer;
            this._placeholders = placeholders;
            this._provider = provider;
            this._log = log;
            this._options = options;
        }

        private async Task AutoChangeLoopAsync(CancellationToken cancellationToken)
        {
            this._log.LogDebug("Starting status randomization loop. Change rate is {ChangeRate}", this._options.CurrentValue.ChangeRate);
            if (this._options.CurrentValue.ChangeRate <= TimeSpan.FromSeconds(10))
                this._log.LogWarning("Change rate is less than 10 seconds!");

            while (!cancellationToken.IsCancellationRequested)
            {
                RandomStatusOptions options = this._options.CurrentValue;

                while (this._client.ConnectionState != ConnectionState.Connected)
                {
                    this._log.LogTrace("Client not connected, waiting");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
                }

                DateTime nextChangeUtc = this._lastChangeUtc + this._options.CurrentValue.ChangeRate;
                TimeSpan remainingWait = nextChangeUtc - DateTime.UtcNow;
                if (remainingWait > TimeSpan.Zero)
                    await Task.Delay(remainingWait, cancellationToken).ConfigureAwait(false);
                await this.RandomizeStatusAsync(cancellationToken).ConfigureAwait(false);
                this._log.LogTrace("Next status change: {ChangeTime}", this._lastChangeUtc + options.ChangeRate);
                await Task.Delay(options.ChangeRate, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<Status> RandomizeStatusAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (this._client.CurrentUser == null || this._client.ConnectionState != ConnectionState.Connected)
                return null;

            IEnumerable<Status> statuses = await this._provider.GetAllAsync(cancellationToken).ConfigureAwait(false);
            statuses = statuses.Where(s => s.IsEnabled);
            if (!statuses.Any())
                return null;

            Status status = this._randomizer.GetRandomValue(statuses);
            if (status == null)
                return null;

            try
            {
                string text = status.Text;
                if (!string.IsNullOrWhiteSpace(status.Text))
                {
                    text = await this._placeholders.ConvertPlaceholdersAsync(status.Text, cancellationToken).ConfigureAwait(false);
                    this._log.LogDebug("Changing status to `{Status}`", text);
                }
                else
                    this._log.LogDebug("Clearing status");
                await this._client.SetGameAsync(text, status.Link, status.ActivityType).ConfigureAwait(false);
                return status;
            }
            catch (Exception ex) when (ex.LogAsError(this._log, "Failed changing status to {Status}", status))
            {
                return null;
            }
            finally
            {
                this._lastChangeUtc = DateTime.UtcNow;
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken).ConfigureAwait(false);
            _ = Task.Run(async () => await this.AutoChangeLoopAsync(base.CancellationToken), base.CancellationToken);
        }
    }
}
