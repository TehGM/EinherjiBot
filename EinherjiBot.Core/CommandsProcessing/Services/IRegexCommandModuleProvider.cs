namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    public interface IRegexCommandModuleProvider
    {
        RegexCommandModule GetModuleInstance(RegexCommandInstance commandInstance);
    }
}
