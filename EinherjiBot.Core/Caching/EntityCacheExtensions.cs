namespace TehGM.EinherjiBot.Caching
{
    public static class EntityCacheExtensions
    {
        public static void RemoveWhere<TKey, TEntity>(this IEntityCache<TKey, TEntity> cache, Func<CachedEntity<TKey, TEntity>, bool> predicate)
        {
            IEnumerable<CachedEntity<TKey, TEntity>> selectedEntities = cache.Find(predicate);
            foreach (CachedEntity<TKey, TEntity> entity in selectedEntities)
                cache.Remove(entity.Key);
        }
    }
}
