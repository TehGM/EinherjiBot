namespace TehGM.EinherjiBot.Security.API
{
    public interface IUserFeatureProvider
    {
        Task<IEnumerable<string>> GetForUserAsync(IDiscordAuthContext context, CancellationToken cancellationToken = default);
    }
}
