using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.Utilities
{
    public class DelayedInvoker : IDisposable
    {
        private CancellationTokenSource _delayedSaveCts;

        public bool IsInvokationDelayed => _delayedSaveCts != null;

        public async Task InvokeDelayedAsync(TimeSpan delay, Action callback)
        {
            if (IsInvokationDelayed)
                return;
            _delayedSaveCts = new CancellationTokenSource();
            CancellationToken ct = _delayedSaveCts.Token;
            await Task.Delay(delay, ct);
            if (ct.IsCancellationRequested)
                return;
            InvokeNow(callback);
        }

        public void InvokeNow(Action callback)
        {
            Cancel();
            callback();
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
