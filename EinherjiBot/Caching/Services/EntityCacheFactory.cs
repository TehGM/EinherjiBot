namespace TehGM.EinherjiBot.Caching.Services
{
    public class EntityCacheFactory<TEntity> : EntityCacheFactory<object, TEntity>, IEntityCache<TEntity> { }

    public class EntityCacheFactory<TKey, TEntity> : IEntityCache<TKey, TEntity>
    {
        private readonly IEntityCache<TKey, TEntity> _cache;

        public EntityCacheFactory()
        {
            this._cache = new EntityCache<TKey, TEntity>();
        }

        #region Auto-Generated: implement interface through _cache
        public int CachedCount => this._cache.CachedCount;

        public ICachedEntityExpiration DefaultExpiration { get => this._cache.DefaultExpiration; set => this._cache.DefaultExpiration = value; }

        public void AddOrReplace(TKey key, TEntity entity, ICachedEntityExpiration expiration)
        {
            this._cache.AddOrReplace(key, entity, expiration);
        }

        public void Clear()
        {
            this._cache.Clear();
        }

        public void ClearExpired()
        {
            this._cache.ClearExpired();
        }

        public void Remove(TKey key)
        {
            this._cache.Remove(key);
        }

        public IEnumerable<CachedEntity<TKey, TEntity>> Scan(Func<CachedEntity<TKey, TEntity>, bool> predicate)
        {
            return this._cache.Scan(predicate);
        }

        public bool TryGet(TKey key, out TEntity result)
        {
            return this._cache.TryGet(key, out result);
        }
        #endregion
    }
}
