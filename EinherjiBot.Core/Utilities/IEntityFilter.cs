namespace TehGM.EinherjiBot
{
    public interface IEntityFilter<T>
    {
        bool Check(T entity);
    }

    public static class EntityFilterExtensions
    {
        public static IEnumerable<T> Filter<T>(this IEntityFilter<T> filter, IEnumerable<T> entities)
            => entities.Where(filter.Check);

        public static IEnumerable<TOutput> Filter<TInput, TOutput>(this IEntityFilter<TInput> filter, IEnumerable<TInput> entities) where TOutput : TInput
            => filter.Filter<TInput>(entities).Cast<TOutput>();
    }
}
