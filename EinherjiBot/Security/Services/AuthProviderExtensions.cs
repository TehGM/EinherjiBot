using Discord;

namespace TehGM.EinherjiBot.Security
{
    public static class AuthProviderExtensions
    {
        public static Task<IDiscordAuthContext> FromInteractionAsync(this IDiscordAuthProvider provider, IDiscordInteraction interaction, CancellationToken cancellationToken = default)
            => provider.GetAsync(interaction.User.Id, interaction.GuildId, cancellationToken);
        public static Task<IDiscordAuthContext> FromMessageAsync(this IDiscordAuthProvider provider, IMessage message, CancellationToken cancellationToken = default)
        {
            ulong? guildID = (message.Channel as IGuildChannel)?.GuildId;
            return provider.GetAsync(message.Author.Id, guildID, cancellationToken);
        }
    }
}
