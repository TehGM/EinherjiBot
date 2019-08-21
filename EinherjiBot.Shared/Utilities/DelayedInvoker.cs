using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.Utilities
{
    public class DelayedInvoker : IDisposable
    {
        private CancellationTokenSource _delayedInvokeTokenSource;

        public bool IsInvokationDelayed => _delayedInvokeTokenSource != null;

        public async Task InvokeDelayedAsync(TimeSpan delay, Action callback)
        {
            if (IsInvokationDelayed)
                return;
            _delayedInvokeTokenSource = new CancellationTokenSource();
            CancellationToken ct = _delayedInvokeTokenSource.Token;
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
            _delayedInvokeTokenSource?.Cancel();
            _delayedInvokeTokenSource = null;
        }

        void IDisposable.Dispose()
            => Cancel();
    }
}
