using Microsoft.Extensions.Hosting;

namespace TehGM.EinherjiBot
{
    public abstract class AutostartService : IHostedService, IDisposable
    {
        private CancellationTokenSource _cts;

        protected CancellationToken CancellationToken => this._cts?.Token ?? default;

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            this._cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            return Task.CompletedTask;
        }

        public virtual Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public virtual void Dispose()
        {
            try { this._cts?.Cancel(); } catch { }
            try { this._cts?.Dispose(); } catch { }
        }
    }
}
