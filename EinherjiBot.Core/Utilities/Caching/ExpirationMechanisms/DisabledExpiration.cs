using TehGM.EinherjiBot.Caching.Services;

namespace TehGM.EinherjiBot.Caching
{
    public struct DisabledExpiration : ICachedEntityExpiration
    {
        public bool IsExpired<TKey, TValue>(CachedEntity<TKey, TValue> entity)
            => false;
    }
}
