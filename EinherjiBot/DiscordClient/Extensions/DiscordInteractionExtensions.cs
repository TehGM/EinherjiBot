using Discord;
using Discord.WebSocket;

namespace TehGM.EinherjiBot
{
    internal static class DiscordInteractionExtensions
    {
        public static Task ClearAndUpdateAsync(this SocketMessageComponent interaction, Action<MessageProperties> func, RequestOptions options = null)
            => interaction.UpdateAsync(msg =>
            {
                msg.Components = null;
                msg.Embed = null;
                msg.AllowedMentions = null;
                msg.Content = null;
                msg.Embeds = null;
                msg.Flags = new Optional<MessageFlags?>(MessageFlags.None);

                func?.Invoke(msg);
            }, options);
    }
}
