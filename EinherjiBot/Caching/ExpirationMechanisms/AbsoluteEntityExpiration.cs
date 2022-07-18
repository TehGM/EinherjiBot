using TehGM.EinherjiBot.Caching.Services;

namespace TehGM.EinherjiBot.Caching
{
    public struct AbsoluteEntityExpiration : ICachedEntityExpiration
    {
        public DateTime ExpirationTime { get; }

        public AbsoluteEntityExpiration(DateTime expirationTime)
        {
            this.ExpirationTime = expirationTime.ToUniversalTime();
        }

        public bool IsExpired<TKey, TValue>(CachedEntity<TKey, TValue> entity)
            => DateTime.UtcNow > this.ExpirationTime;
    }
}
