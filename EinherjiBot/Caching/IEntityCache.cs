using TehGM.EinherjiBot.Caching.Services;

namespace TehGM.EinherjiBot.Caching
{
    public interface IEntityCache<TEntity> : IEntityCache<object, TEntity> { }

    public interface IEntityCache<TKey, TEntity>
    {
        int CachedCount { get; }
        ICachedEntityExpiration DefaultExpiration { get; set; }

        TEntity Get(TKey key);
        void AddOrReplace(TKey key, TEntity entity, ICachedEntityExpiration expiration);
        IEnumerable<CachedEntity<TKey, TEntity>> Scan(Func<CachedEntity<TKey, TEntity>, bool> predicate);
        void Clear();
        void ClearExpired();
        void Remove(TKey key);
    }
}
