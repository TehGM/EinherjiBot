using DSharpPlus.Entities;
using TehGM.EinherjiBot;

namespace Discord
{
    public static class EmbedBuilderExtensions
    {
        public static EmbedBuilder WithColor(this EmbedBuilder builder, System.Drawing.Color color)
            => builder.WithColor((Color)color);

        public static DiscordEmbedBuilder WithColor(this DiscordEmbedBuilder builder, System.Drawing.Color color)
            => builder.WithColor(color.ToDiscordColor());
    }
}

namespace TehGM.EinherjiBot
{
    public static class EmbedBuilderExtensions
    {
        public static DiscordEmbedBuilder WithColor(this DiscordEmbedBuilder builder, System.Drawing.Color color)
            => builder.WithColor(color.ToDiscordColor());
    }
}
