namespace TehGM.EinherjiBot.Security
{
    public interface IAuthRequiredEntity
    {
        // TODO: this is quite naive right now. Probably want to transition to policies or something.
        bool CanAccess(IDiscordAuthContext context);
        bool CanEdit(IDiscordAuthContext context);
    }
}
