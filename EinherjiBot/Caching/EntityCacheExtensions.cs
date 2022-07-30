using TehGM.EinherjiBot.Caching.Services;

namespace TehGM.EinherjiBot.Caching
{
    public static class EntityCacheExtensions
    {
        public static TEntity Get<TKey, TEntity>(this IEntityCache<TKey, TEntity> cache, TKey key)
        {
            if (cache.TryGet(key, out TEntity result))
                return result;
            return default;
        }

        // keyed
        public static void AddOrReplace<TKey, TEntity>(this IEntityCache<TKey, TEntity> cache, TKey key, TEntity entity)
            => cache.AddOrReplace(key, entity, cache.DefaultExpiration);
        public static void AddOrReplace<TKey, TEntity>(this IEntityCache<TKey, TEntity> cache, TEntity entity, ICachedEntityExpiration expiration) where TEntity : ICacheableEntity<TKey>
            => cache.AddOrReplace(entity.GetCacheKey(), entity, expiration);
        public static void AddOrReplace<TKey, TEntity>(this IEntityCache<TKey, TEntity> cache, TEntity entity) where TEntity : ICacheableEntity<TKey>
            => AddOrReplace(cache, entity, cache.DefaultExpiration);

        public static IEnumerable<TEntity> Find<TKey, TEntity>(this IEntityCache<TKey, TEntity> cache, Func<TEntity, bool> predicate)
            => cache.Scan(e => predicate(e.Entity)).Select(e => e.Entity);

        public static void Remove<TKey, TEntity>(this IEntityCache<TKey, TEntity> cache, TEntity entity) where TEntity : ICacheableEntity<TKey>
            => cache.Remove(entity.GetCacheKey());

        public static void RemoveWhere<TKey, TEntity>(this IEntityCache<TKey, TEntity> cache, Func<TEntity, bool> predicate)
        {
            IEnumerable<CachedEntity<TKey, TEntity>> selectedEntities = cache.Scan(e => predicate(e.Entity));
            foreach (CachedEntity<TKey, TEntity> entity in selectedEntities)
                cache.Remove(entity.Key);
        }


        // object-based
        public static void AddOrReplace<TEntity>(this IEntityCache<object, TEntity> cache, TEntity entity, ICachedEntityExpiration expiration) where TEntity : ICacheableEntity
            => cache.AddOrReplace(entity.GetCacheKey(), entity, expiration);

        public static void AddOrReplace<TEntity>(this IEntityCache<object, TEntity> cache, TEntity entity)
        {
            object key = entity;
            if (entity is ICacheableEntity cacheableEntity)
                key = cacheableEntity.GetCacheKey();
            cache.AddOrReplace(key, entity, cache.DefaultExpiration);
        }

        public static IEnumerable<TEntity> Find<TEntity>(this IEntityCache<object, TEntity> cache, Func<TEntity, bool> predicate)
            => cache.Scan(e => predicate(e.Entity)).Select(e => e.Entity);

        public static void Remove<TEntity>(this IEntityCache<object, TEntity> cache, TEntity entity) where TEntity : ICacheableEntity
            => cache.Remove(entity.GetCacheKey());
    }
}
