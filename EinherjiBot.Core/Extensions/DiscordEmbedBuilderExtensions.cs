using DSharpPlus.Entities;

namespace TehGM.EinherjiBot
{
    public static class DiscordEmbedBuilderExtensions
    {
        public static DiscordEmbedBuilder WithColor(this DiscordEmbedBuilder builder, System.Drawing.Color color)
            => builder.WithColor(color.ToDiscordColor());
    }
}
