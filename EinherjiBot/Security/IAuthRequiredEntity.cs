namespace TehGM.EinherjiBot.Security
{
    public interface IAuthRequiredEntity
    {
        // TODO: this is quite naive right now. Probably want to transition to policies or something.
        bool CanAccess(IAuthContext context);
        bool CanEdit(IAuthContext context);
    }
}
