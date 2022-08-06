using Microsoft.Extensions.DependencyInjection;
using TehGM.EinherjiBot.DiscordClient;

namespace TehGM.EinherjiBot.BotStatus.Services
{
    /// <summary>Background service that periodically scans blog channels for last activity and activates or deactivates them.</summary>
    internal class AutoStatusService : ScopedAutostartService, IDisposable
    {
        private readonly IDiscordConnection _connection;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<BotStatusOptions> _options;

        private DateTime _lastChangeUtc;

        public AutoStatusService(IDiscordConnection connection, IOptionsMonitor<BotStatusOptions> options,
            IServiceProvider services, ILogger<AutoStatusService> log) : base(services)
        {
            this._connection = connection;
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
                BotStatusOptions options = this._options.CurrentValue;

                await this._connection.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                DateTime nextChangeUtc = this._lastChangeUtc + this._options.CurrentValue.ChangeRate;
                TimeSpan remainingWait = nextChangeUtc - DateTime.UtcNow;
                if (remainingWait > TimeSpan.Zero)
                    await Task.Delay(remainingWait, cancellationToken).ConfigureAwait(false);
                try
                {
                    using IServiceScope scope = await base.CreateBotUserScopeAsync(cancellationToken).ConfigureAwait(false);
                    IBotStatusSetter setter = scope.ServiceProvider.GetRequiredService<IBotStatusSetter>();
                    await setter.RandomizeStatusAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex.LogAsError(this._log, "Error when randomizing status")) { }
                finally
                {
                    this._lastChangeUtc = DateTime.UtcNow;
                    this._log.LogTrace("Next status change: {ChangeTime}", this._lastChangeUtc + options.ChangeRate);
                    await Task.Delay(options.ChangeRate, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken).ConfigureAwait(false);
            _ = Task.Run(async () => await this.AutoChangeLoopAsync(base.CancellationToken), base.CancellationToken);
        }
    }
}
