using System.Globalization;

namespace TehGM.EinherjiBot.Globalization
{
    public interface ITimestampFormatter
    {
        string Format(DateTime timestampUtc, TimestampFormat format, TimeZoneInfo timezone, CultureInfo culture);
    }
}
