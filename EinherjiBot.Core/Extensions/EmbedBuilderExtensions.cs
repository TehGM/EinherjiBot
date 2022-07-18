namespace Discord
{
    public static class EmbedBuilderExtensions
    {
        public static EmbedBuilder WithColor(this EmbedBuilder builder, System.Drawing.Color color)
            => builder.WithColor((Color)color);
    }
}
