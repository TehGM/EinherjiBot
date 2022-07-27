namespace TehGM.EinherjiBot.Security
{
    public interface IAuthProvider
    {
        IAuthContext User { get; }
    }
}
