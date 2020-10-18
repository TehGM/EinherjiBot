using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        public virtual void AddOrReplace(TKey key, TEntity entity, TimeSpan lifetime)
        {
            lock (_cachedEntities)
                _cachedEntities[key] = new CachedEntity<TKey, TEntity>(key, entity, lifetime);
            this.ClearExpired();
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

            this.ClearExpired();

            lock (_cachedEntities)
                return _cachedEntities.Where(pair => predicate(pair.Value)).Select(pair => pair.Value).ToImmutableArray();
        }

        public virtual TEntity Get(TKey key)
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
            }
            this.ClearExpired();
            return result;
        }

        public void Remove(TKey key)
        {
            lock (_cachedEntities)
                _cachedEntities.Remove(key);
            this.ClearExpired();
        }
    }
}
