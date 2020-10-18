namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    public interface IRegexCommandModuleProvider
    {
        object GetModuleInstance(RegexCommandInstance commandInstance);
    }
}
