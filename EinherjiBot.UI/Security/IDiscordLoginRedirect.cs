namespace TehGM.EinherjiBot.UI.Security
{
    public interface IDiscordLoginRedirect
    {
        Task RedirectAsync(CancellationToken cancellationToken = default);
        Task<bool> ValidateStateAsync(string state, CancellationToken cancellationToken = default);
    }
}
