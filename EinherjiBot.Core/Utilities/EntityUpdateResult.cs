namespace TehGM.EinherjiBot
{
    public interface IEntityUpdateResult
    {
        EntityUpdateResultType ResultType { get; }

        static EntityUpdateResult<TEntity> NoChanges<TEntity>(TEntity entity)
            => new EntityUpdateResult<TEntity>(EntityUpdateResultType.NoChanges, entity);
        static EntityUpdateResult<TEntity> Saved<TEntity>(TEntity entity)
            => new EntityUpdateResult<TEntity>(EntityUpdateResultType.Saved, entity);
    }

    public interface IEntityUpdateResult<out TEntity> : IEntityUpdateResult
    {
        TEntity Entity { get; }
    }

    public class EntityUpdateResult<TEntity> : IEntityUpdateResult<TEntity>
    {
        public EntityUpdateResultType ResultType { get; }
        public TEntity Entity { get; }

        public EntityUpdateResult(EntityUpdateResultType result, TEntity entity)
        {
            this.ResultType = result;
            this.Entity = entity;
        }

        public static implicit operator TEntity(EntityUpdateResult<TEntity> result)
            => result.Entity;
    }

    public enum EntityUpdateResultType
    {
        NoChanges,
        Saved
    }

    public static class EntityUpdateResultExtensions
    {
        public static bool NoChanges(this IEntityUpdateResult result)
            => result.ResultType == EntityUpdateResultType.NoChanges;
        public static bool Saved(this IEntityUpdateResult result)
            => result.ResultType == EntityUpdateResultType.Saved;
    }
}
