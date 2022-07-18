using TehGM.EinherjiBot.Caching.Services;

namespace TehGM.EinherjiBot.Caching
{
    public struct TimeSpanEntityExpiration : ICachedEntityExpiration
    {
        public TimeSpan Lifetime { get; }

        public TimeSpanEntityExpiration()
            : this(TimeSpan.FromMinutes(10)) { }
        public TimeSpanEntityExpiration(TimeSpan lifetime)
        {
            if (lifetime <= TimeSpan.Zero)
                throw new ArgumentException("Entity lifetime must be a non-zero positive value", nameof(lifetime));

            this.Lifetime = lifetime;
        }

        public bool IsExpired<TKey, TValue>(CachedEntity<TKey, TValue> entity)
        {
            DateTime expirationTime = entity.CachingTimeUtc + this.Lifetime;
            return DateTime.UtcNow > expirationTime;
        }
    }
}
