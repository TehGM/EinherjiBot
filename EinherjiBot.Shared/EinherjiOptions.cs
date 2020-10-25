using System.Drawing;

namespace TehGM.EinherjiBot
{
    public class EinherjiOptions
    {
        public string FailureSymbol { get; set; } = "\u274C";
        public string SuccessSymbol { get; set; } = "\u2705";
        public Color EmbedErrorColor { get; set; } = Color.FromArgb(255, 0, 0);
        public Color EmbedSuccessColor { get; set; } = Color.FromArgb(0, 255, 0);
        public ulong AuthorID { get; set; } = 247081094799687682;
    }
}
