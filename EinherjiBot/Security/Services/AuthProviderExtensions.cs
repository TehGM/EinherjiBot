using Discord;

namespace TehGM.EinherjiBot.Security
{
    public static class AuthProviderExtensions
    {
        public static Task<IDiscordAuthContext> FromInteractionAsync(this IDiscordAuthProvider provider, IDiscordInteraction interaction, CancellationToken cancellationToken = default)
            => provider.GetAsync(interaction.User.Id, interaction.GuildId, cancellationToken);
    }
}
