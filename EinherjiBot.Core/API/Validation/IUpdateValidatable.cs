namespace TehGM.EinherjiBot.API
{
    public interface IUpdateValidatable<TEntity>
    {
        IEnumerable<string> ValidateForUpdate(TEntity existing);
    }
}
