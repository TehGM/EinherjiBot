namespace TehGM.EinherjiBot
{
    public static class LockProviderExtensions
    {
        public static Task WaitAsync(this ILockProvider provider, CancellationToken cancellationToken = default)
        {
            SemaphoreSlim @lock = provider.Get();
            return @lock.WaitAsync(cancellationToken);
        }

        public static async Task ExecuteAsync(this ILockProvider provider, Action lockedActions, CancellationToken cancellationToken = default)
        {
            await WaitAsync(provider, cancellationToken).ConfigureAwait(false);
            try
            {
                lockedActions.Invoke();
            }
            finally
            {
                provider.Release();
            }
        }

        public static void ExecuteSync<TCaller>(this ILockProvider provider, Action lockedActions)
        {
            SemaphoreSlim @lock = provider.Get();
            lock (@lock)
            {
                lockedActions.Invoke();
            }
        }
    }
}
