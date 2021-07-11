using System;
using System.Collections.Generic;

namespace TehGM.EinherjiBot.Caching
{
    public interface IEntityCache<TKey, TEntity>
    {
        int CachedCount { get; }

        void Clear();
        void ClearExpired();
        IEnumerable<CachedEntity<TKey, TEntity>> Find(Func<CachedEntity<TKey, TEntity>, bool> predicate);
        void AddOrReplace(TKey key, TEntity entity, TimeSpan lifetime);
        void Remove(TKey key);
        TEntity Get(TKey key);
    }
}
