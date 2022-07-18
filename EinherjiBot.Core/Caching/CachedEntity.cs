namespace TehGM.EinherjiBot.Caching
{
    public class CachedEntity<TKey, TEntity>
    {
        public TKey Key { get; }
        public TEntity Entity { get; }
        public DateTime CachingTimeUtc { get; }
        public DateTime ExpirationTimeUtc { get; }
        public bool IsExpired => DateTime.UtcNow > ExpirationTimeUtc;

        public CachedEntity(TKey key, TEntity entity, TimeSpan lifetime)
        {
            this.Key = key;
            this.Entity = entity;
            this.CachingTimeUtc = DateTime.UtcNow;
            this.ExpirationTimeUtc = this.CachingTimeUtc + lifetime;
        }

        public static implicit operator TEntity(CachedEntity<TKey, TEntity> cached)
        {
            if (cached == null)
                return default;
            return cached.Entity;
        }
    }
}
