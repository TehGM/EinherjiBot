using System.Collections.Immutable;

namespace TehGM.EinherjiBot.Caching.Services
{
    public class EntityCache<TKey, TEntity> : IEntityCache<TKey, TEntity>
    {
        public int CachedCount => _cachedEntities.Count;
        public ICachedEntityExpiration DefaultExpiration { get; set; } 
            = new TimeSpanEntityExpiration(TimeSpan.FromMinutes(10));

        private readonly IDictionary<TKey, CachedEntity<TKey, TEntity>> _cachedEntities;

        public EntityCache() : this(EqualityComparer<TKey>.Default) { }
        public EntityCache(IEqualityComparer<TKey> comparer)
        {
            this._cachedEntities = new Dictionary<TKey, CachedEntity<TKey, TEntity>>(comparer);
        }

        public bool TryGet(TKey key, out TEntity result)
        {
            bool found = false;
            result = default;
            lock (this._cachedEntities)
            {
                if (this._cachedEntities.TryGetValue(key, out CachedEntity<TKey, TEntity> entity))
                {
                    if (!entity.IsExpired)
                    {
                        result = entity.Entity;
                        entity.Touch();
                        found = true;
                    }
                    else
                        this._cachedEntities.Remove(key);
                }
                this.ClearExpiredInternal();
            }
            return found;
        }

        public void AddOrReplace(TKey key, TEntity entity, ICachedEntityExpiration expiration)
        {
            lock (this._cachedEntities)
            {
                this._cachedEntities[key] = new CachedEntity<TKey, TEntity>(key, entity, expiration);
                this.ClearExpiredInternal();
            }
        }

        public IEnumerable<CachedEntity<TKey, TEntity>> Scan(Func<CachedEntity<TKey, TEntity>, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            lock (this._cachedEntities)
            {
                this.ClearExpiredInternal();
                IEnumerable<CachedEntity<TKey, TEntity>> results = this._cachedEntities
                    .Where(pair => predicate(pair.Value))
                    .Select(pair =>
                    {
                        pair.Value.Touch();
                        return pair.Value;
                    });
                return results.ToImmutableArray();
            }
        }

        public void Clear()
        {
            lock (this._cachedEntities)
                this._cachedEntities.Clear();
        }

        public void Remove(TKey key)
        {
            lock (this._cachedEntities)
            {
                this._cachedEntities.Remove(key);
                this.ClearExpiredInternal();
            }
        }

        public void ClearExpired()
        {
            lock (this._cachedEntities)
                this.ClearExpiredInternal();
        }

        private void ClearExpiredInternal()
        {
            IEnumerable<KeyValuePair<TKey, CachedEntity<TKey, TEntity>>> expired = this._cachedEntities.Where(e => e.Value.IsExpired);
            foreach (KeyValuePair<TKey, CachedEntity<TKey, TEntity>> e in expired)
                this._cachedEntities.Remove(e.Key);
        }
    }
}
