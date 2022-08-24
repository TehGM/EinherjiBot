namespace TehGM.EinherjiBot.Globalization
{
    public enum TimestampFormat
    {
        /// <summary>Full date including DOW and time, but no seconds: Saturday, 01 January 2022 09:01.</summary>
        FullDateTime,
        /// <summary>Full date including DOW and time, with seconds: Saturday, 01 January 2022 09:01:33.</summary>
        FullDateTimeWithSeconds,
        /// <summary>Full date including DOW: Saturday, 01 January 2022.</summary>
        FullDate,
        /// <summary>Long date: 01 January 2022.</summary>
        LongDate,
        /// <summary>Time with seconds: 09:01:33.</summary>
        LongTime,
        /// <summary>Long date and time: 01 January 2022 09:01.</summary>
        LongDateTime,
        /// <summary>Long date and time, with seconds: 01 January 2022 09:01:33.</summary>
        LongDateTimeWithSeconds,
        /// <summary>Short date: 01.01.2022.</summary>
        ShortDate,
        /// <summary>Time without seconds: 09:01.</summary>
        ShortTime,
        /// <summary>Short date and time: 01.01.2022 09:01.</summary>
        ShortDateTime,
        /// <summary>Short date and time, with seconds: 01.01.2022 09:01:33.</summary>
        ShortDateTimeWithSeconds,
        /// <summary>Relative time, rounded to biggest component: 3 weeks ago.</summary>
        RelativeLowPrecision,
        /// <summary>Relative time, rounded to hours: 2 weeks, 2 days and 3 hours ago.</summary>
        RelativeMediumPrecision,
        /// <summary>Relative time, with up seconds precision: 2 weeks, 2 days, 3 hours, 15 minutes and 33 seconds ago.</summary>
        RelativeHighPrecision
    }
}
