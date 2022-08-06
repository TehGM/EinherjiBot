namespace TehGM.EinherjiBot
{
    public interface ILockProvider<TCaller> : ILockProvider { }

    public interface ILockProvider
    {
        SemaphoreSlim Get();
        void Release();
    }
}
