namespace TehGM.EinherjiBot.Caching.Services
{
    public class CachedEntity<TKey, TEntity>
    {
        public TKey Key { get; }
        public TEntity Entity { get; }
        public DateTime CachingTimeUtc { get; }
        public DateTime AccessTimeUtc { get; private set; }
        public ICachedEntityExpiration Expiration { get; }

        public bool IsExpired => this.Expiration.IsExpired(this);

        public CachedEntity(TKey key, TEntity entity, ICachedEntityExpiration expiration)
        {
            this.Key = key;
            this.Entity = entity;
            this.CachingTimeUtc = DateTime.UtcNow;
            this.Expiration = expiration;
            this.Touch();
        }

        public void Touch()
            => this.AccessTimeUtc = DateTime.UtcNow;

        public static implicit operator TEntity(CachedEntity<TKey, TEntity> cached)
        {
            if (cached == null)
                return default;
            return cached.Entity;
        }
    }
}
