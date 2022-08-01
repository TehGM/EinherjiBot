using TehGM.EinherjiBot.Caching.Services;

namespace TehGM.EinherjiBot.Caching
{
    public interface ICachedEntityExpiration
    {
        bool IsExpired<TKey, TValue>(CachedEntity<TKey, TValue> entity);
    }
}
