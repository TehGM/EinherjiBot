namespace TehGM.EinherjiBot.Services
{
    public class LockProvider<TCaller> : ILockProvider<TCaller>, IDisposable
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public SemaphoreSlim Get()
            => this._lock;

        public void Release()
            => this._lock.Release();

        public void Dispose()
        {
            try { this._lock?.Dispose(); } catch { }
        }
    }
}
