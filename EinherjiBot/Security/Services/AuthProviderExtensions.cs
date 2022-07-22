using Discord;

namespace TehGM.EinherjiBot.Security
{
    public static class AuthProviderExtensions
    {
        public static Task<IAuthContext> FromInteractionAsync(this IAuthProvider provider, IDiscordInteraction interaction, CancellationToken cancellationToken = default)
            => provider.GetAsync(interaction.User.Id, interaction.GuildId, cancellationToken);
    }
}
