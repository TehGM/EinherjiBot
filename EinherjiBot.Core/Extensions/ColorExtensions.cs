﻿using System.Drawing;
using DSharpPlus.Entities;

namespace TehGM.EinherjiBot
{
    public static class ColorExtensions
    {
        public static DiscordColor ToDiscordColor(this Color color)
            => new DiscordColor(color.R, color.G, color.B);
    }
}
