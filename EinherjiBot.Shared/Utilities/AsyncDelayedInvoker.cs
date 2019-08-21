using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.Utilities
{
    public class AsyncDelayedInvoker : IDisposable
    {
        private CancellationTokenSource _delayedInvokeTokenSource;

        public bool IsInvokationDelayed => _delayedInvokeTokenSource != null;

        public async Task InvokeDelayedAsync(TimeSpan delay, Func<Task> callback)
        {
            if (IsInvokationDelayed)
                return;
            _delayedInvokeTokenSource = new CancellationTokenSource();
            CancellationToken ct = _delayedInvokeTokenSource.Token;
            await Task.Delay(delay, ct);
            if (ct.IsCancellationRequested)
                return;
            await InvokeNowAsync(callback);
        }

        public Task InvokeNowAsync(Func<Task> callback)
        {
            Cancel();
            return callback();
        }

        public void Cancel()
        {
            _delayedInvokeTokenSource?.Cancel();
            _delayedInvokeTokenSource = null;
        }

        void IDisposable.Dispose()
            => Cancel();
    }
}
