using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.Utilities
{
    public class AsyncDelayedInvoker : IDisposable
    {
        private CancellationTokenSource _delayedSaveCts;

        public bool IsInvokationDelayed => _delayedSaveCts != null;

        public async Task InvokeDelayedAsync(TimeSpan delay, Func<Task> callback)
        {
            if (IsInvokationDelayed)
                return;
            _delayedSaveCts = new CancellationTokenSource();
            CancellationToken ct = _delayedSaveCts.Token;
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
            _delayedSaveCts?.Cancel();
            _delayedSaveCts = null;
        }

        void IDisposable.Dispose()
            => Cancel();
    }
}
