using System.Globalization;

namespace TehGM.EinherjiBot.Globalization
{
    public static class TimestampFormatterExtensions
    {
        public static string Format(this ITimestampFormatter formatter, DateTime timestampUtc, TimestampFormat format, TimeZoneInfo timezone)
            => formatter.Format(timestampUtc, format, timezone, CultureInfo.CurrentCulture);
        public static string Format(this ITimestampFormatter formatter, DateTime timestampUtc, TimestampFormat format, CultureInfo culture)
            => formatter.Format(timestampUtc, format, TimeZoneInfo.Local, culture);
        public static string Format(this ITimestampFormatter formatter, DateTime timestampUtc, TimestampFormat format)
            => formatter.Format(timestampUtc, format, TimeZoneInfo.Local, CultureInfo.CurrentCulture);
    }
}
