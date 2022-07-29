using Discord;

namespace TehGM.EinherjiBot.Intel
{
    public interface IIntelEmbedBuilder
    {
        Task<Embed> BuildUserEmbedAsync(IUser user, IGuild guild, CancellationToken cancellationToken = default);
        Task<Embed> BuildGuildEmbedAsync(IGuild guild, CancellationToken cancellationToken = default);
    }
}
