namespace TehGM.EinherjiBot
{
    public interface ICacheableEntity<TKey>
    {
        TKey GetCacheKey();
    }

    public interface ICacheableEntity
    {
        object GetCacheKey();
    }
}
