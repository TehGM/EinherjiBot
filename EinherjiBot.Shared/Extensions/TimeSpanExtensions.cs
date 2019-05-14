using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.EinherjiBot.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToFriendlyString(this TimeSpan span)
               => $"{(int)span.TotalHours} hours {(int)span.Minutes} minutes {(int)span.Seconds} seconds";
    }
}
