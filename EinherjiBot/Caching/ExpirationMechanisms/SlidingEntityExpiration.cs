using TehGM.EinherjiBot.Caching.Services;

namespace TehGM.EinherjiBot.Caching
{
    public struct SlidingEntityExpiration : ICachedEntityExpiration
    {
        public TimeSpan SlidingLifetime { get; }

        public SlidingEntityExpiration()
            : this(TimeSpan.FromMinutes(10)) { }
        public SlidingEntityExpiration(TimeSpan slidingLifetime)
        {
            if (slidingLifetime <= TimeSpan.Zero)
                throw new ArgumentException("Entity lifetime must be a non-zero positive value", nameof(slidingLifetime));

            this.SlidingLifetime = slidingLifetime;
        }

        public bool IsExpired<TKey, TValue>(CachedEntity<TKey, TValue> entity)
        {
            DateTime expirationTime = entity.AccessTimeUtc + this.SlidingLifetime;
            return DateTime.UtcNow > expirationTime;
        }

        public static implicit operator SlidingEntityExpiration(TimeSpan lifetime)
            => new SlidingEntityExpiration(lifetime);
    }
}
