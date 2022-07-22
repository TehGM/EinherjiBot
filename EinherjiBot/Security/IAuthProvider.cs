using Discord;

namespace TehGM.EinherjiBot.Security
{
    public interface IAuthProvider
    {
        Task<IAuthContext> GetAsync(ulong userID, ulong? guildID, CancellationToken cancellationToken = default);

        IAuthContext Current { get; set; }
    }
}
