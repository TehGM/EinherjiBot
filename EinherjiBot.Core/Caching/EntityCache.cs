using System.Collections.Immutable;

namespace TehGM.EinherjiBot.Caching.Services
{
    public class EntityCache<TKey, TEntity> : IEntityCache<TKey, TEntity>
    {
        private readonly IDictionary<TKey, CachedEntity<TKey, TEntity>> _cachedEntities;

        public int CachedCount => _cachedEntities.Count;

        public EntityCache(IEqualityComparer<TKey> comparer)
        {
            this._cachedEntities = new Dictionary<TKey, CachedEntity<TKey, TEntity>>(comparer);
        }

        public EntityCache() : this(EqualityComparer<TKey>.Default) { }

        public void AddOrReplace(TKey key, TEntity entity, TimeSpan lifetime)
        {
            lock (_cachedEntities)
            {
                _cachedEntities[key] = new CachedEntity<TKey, TEntity>(key, entity, lifetime);
                this.ClearExpiredInternal();
            }
        }

        public void Clear()
        {
            lock (_cachedEntities)
                _cachedEntities.Clear();
        }

        public IEnumerable<CachedEntity<TKey, TEntity>> Find(Func<CachedEntity<TKey, TEntity>, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            lock (_cachedEntities)
            {
                this.ClearExpiredInternal();
                return _cachedEntities.Where(pair => predicate(pair.Value)).Select(pair => pair.Value).ToImmutableArray();
            }
        }

        public TEntity Get(TKey key)
        {
            TEntity result = default;
            lock (_cachedEntities)
            {
                if (_cachedEntities.TryGetValue(key, out CachedEntity<TKey, TEntity> entity))
                {
                    if (!entity.IsExpired)
                        result = entity.Entity;
                    else _cachedEntities.Remove(key);
                }
                this.ClearExpiredInternal();
            }
            return result;
        }

        public void Remove(TKey key)
        {
            lock (_cachedEntities)
            {
                _cachedEntities.Remove(key);
                this.ClearExpiredInternal();
            }
        }

        public void ClearExpired()
        {
            lock (_cachedEntities)
            {
                this.ClearExpiredInternal();
            }
        }

        private void ClearExpiredInternal()
        {
            IEnumerable<KeyValuePair<TKey, CachedEntity<TKey, TEntity>>> expired = this._cachedEntities.Where(e => e.Value.IsExpired);
            foreach (KeyValuePair<TKey, CachedEntity<TKey, TEntity>> e in expired)
                _cachedEntities.Remove(e.Key);
        }
    }
}
