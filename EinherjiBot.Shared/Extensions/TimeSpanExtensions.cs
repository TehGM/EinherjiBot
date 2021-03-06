﻿using System;

namespace TehGM.EinherjiBot
{
    public static class TimeSpanExtensions
    {
        public static string ToFriendlyString(this TimeSpan span)
               => $"{(int)span.TotalHours} hours, {(int)span.Minutes} minutes and {(int)span.Seconds} seconds";
        public static string ToLongFriendlyString(this TimeSpan span)
               => $"{span.Days} days, {(int)span.Hours} hours, {(int)span.Minutes} minutes and {(int)span.Seconds} seconds";
        public static string ToShortFriendlyString(this TimeSpan span)
               => $"{(int)span.TotalMinutes} minutes and {(int)span.Seconds} seconds";
    }
}
