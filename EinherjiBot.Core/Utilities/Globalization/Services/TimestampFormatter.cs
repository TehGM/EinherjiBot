using NodaTime;
using System.Globalization;

namespace TehGM.EinherjiBot.Globalization.Services
{
    public class TimestampFormatter : ITimestampFormatter
    {
        public string Format(DateTime timestampUtc, TimestampFormat format, TimeZoneInfo timezone, CultureInfo culture)
        {
            DateTime localTimestamp = TimeZoneInfo.ConvertTimeFromUtc(timestampUtc, timezone);
            switch (format)
            {
                case TimestampFormat.FullDateTime:
                    return localTimestamp.ToString("f", culture);
                case TimestampFormat.FullDateTimeWithSeconds:
                    return localTimestamp.ToString("F", culture);
                case TimestampFormat.FullDate:
                    return localTimestamp.ToString("D", culture);
                case TimestampFormat.LongDate:
                    return GetLongDate();
                case TimestampFormat.LongTime:
                    return GetLongTime();
                case TimestampFormat.LongDateTime:
                    return $"{GetLongDate()} {GetShortTime()}";
                case TimestampFormat.LongDateTimeWithSeconds:
                    return $"{GetLongTime()} {GetLongTime()}";
                case TimestampFormat.ShortDate:
                    return GetShortTime();
                case TimestampFormat.ShortTime:
                    return GetShortTime();
                case TimestampFormat.ShortDateTime:
                    return $"{GetShortDate()} {GetShortTime()}";
                case TimestampFormat.ShortDateTimeWithSeconds:
                    return $"{GetShortDate()} {GetLongTime()}";
                case TimestampFormat.RelativeLowPrecision:
                case TimestampFormat.RelativeMediumPrecision:
                case TimestampFormat.RelativeHighPrecision:
                    return this.FormatRelative(timestampUtc, format);
                default:
                    throw new ArgumentException($"Unsupported timestamp format {format}.", nameof(format));
            }

            string GetShortDate()
                => localTimestamp.ToString("d", culture);
            string GetLongDate()
                => $"{localTimestamp.ToString("M", culture)} {localTimestamp.ToString("yyyy", culture)}";
            string GetShortTime()
                => localTimestamp.ToString("t", culture);
            string GetLongTime()
                => localTimestamp.ToString("T", culture);
        }

        private string FormatRelative(DateTime timestampUtc, TimestampFormat format)
        {
            Period diff = Period.Between(LocalDateTime.FromDateTime(timestampUtc), LocalDateTime.FromDateTime(DateTime.UtcNow), PeriodUnits.AllDateUnits | PeriodUnits.HourMinuteSecond);

            if (diff == Period.Zero)
                return "now";

            bool isInPast = (DateTime.UtcNow - timestampUtc) > TimeSpan.Zero;
            IEnumerable<string> components = this.BuildRelativeComponents(diff, format);
            int componentsCount = components.Count();
            string value;

            if (componentsCount == 0)
                value = "less than an hour";
            else if (componentsCount == 1)
                value = components.First().ToString();
            else
                value = $"{string.Join(", ", components.Take(componentsCount - 1))} and {components.Last()}";

            if (isInPast)
                return $"{value} ago";
            else
                return $"in {value}";
        }

        private IEnumerable<string> BuildRelativeComponents(Period diff, TimestampFormat format)
        {
            int maxComponents = format == TimestampFormat.RelativeLowPrecision ? 1
                : format == TimestampFormat.RelativeMediumPrecision ? 4
                : format == TimestampFormat.RelativeHighPrecision ? 7
                : throw new ArgumentException($"Timestamp format {format} is not valid for relative formatting", nameof(format));

            // for custom formatting, we just add components to collection
            // this way we can easily add each conditionally, and then simply join them
            List<string> components = new List<string>(maxComponents);

            if (diff.Years > 0)
            {
                AddComponent(diff.Years, "year");
                if (format == TimestampFormat.RelativeLowPrecision && components.Any())
                    return components;
            }
            if (diff.Months > 0 || components.Any())
            {
                AddComponent(diff.Months, "month");
                if (format == TimestampFormat.RelativeLowPrecision && components.Any())
                    return components;
            }

            // don't include weeks if there are years or months to avoid unnecessary clutter
            if (diff.Weeks > 0 && !components.Any())
            {
                AddComponent(diff.Weeks, "week");
                if (format == TimestampFormat.RelativeLowPrecision && components.Any())
                    return components;
            }

            if (diff.Days > 0 || components.Any())
            {
                AddComponent(diff.Days, "day");
                if (format == TimestampFormat.RelativeLowPrecision && components.Any())
                    return components;
            }

            // another special treatment - if there are years, months or weeks, don't include for medium precision
            // also, high precision should include hour regardless
            // again just to avoid unnecessary clutter
            if (diff.Hours > 0 || components.Any())
            {
                if (format != TimestampFormat.RelativeMediumPrecision || (diff.Years + diff.Months + diff.Weeks) == 0)
                    AddComponent(diff.Hours, "hour");
                if (format == TimestampFormat.RelativeLowPrecision && components.Any())
                    return components;
                if (format == TimestampFormat.RelativeMediumPrecision && components.Any())
                    return components;
            }

            // yet another special case: for low and medium precision, any value under 1 hour should be "less than 1 hour ago" etc
            // to indicate this, we simply return empty components list
            if (format == TimestampFormat.RelativeLowPrecision || format == TimestampFormat.RelativeMediumPrecision)
                return Enumerable.Empty<string>();

            if (diff.Minutes > 0 || components.Any())
                AddComponent(diff.Minutes, "minute");

            // always include seconds, to ensure there's always at least one component to display
            AddComponent(diff.Seconds, "second");
            return components;

            void AddComponent(long value, string text)
            {
                if (value == 1)
                    components.Add($"{value} {text}");
                // since we currently offer Einherji in English only, pluralization is easy
                else
                    components.Add($"{value} {text}s");
            }
        }
    }
}
