namespace TehGM.EinherjiBot.API
{
    public interface ICreateValidatable
    {
        IEnumerable<string> ValidateForCreation();
    }
}
