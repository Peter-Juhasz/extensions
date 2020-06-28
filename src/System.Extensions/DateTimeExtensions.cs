using System.Xml;

namespace System
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset StartOfWeek(this DateTimeOffset dt, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (dt.DayOfWeek - firstDayOfWeek)) % 7;
            return new DateTimeOffset(dt.AddDays(-1 * diff).Date, dt.Offset);
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (dt.DayOfWeek - firstDayOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static string ToRfc3339Format(this DateTimeOffset dateTime) => XmlConvert.ToString(dateTime);

        public static DateTime Floor(this DateTime date, TimeSpan span) => new DateTime(date.Ticks / span.Ticks * span.Ticks);
        public static DateTimeOffset Floor(this DateTimeOffset date, TimeSpan span) => new DateTimeOffset(date.Ticks / span.Ticks * span.Ticks, date.Offset);

        public static DateTime Round(this DateTime date, TimeSpan span) => new DateTime((date.Ticks + (span.Ticks / 2) + 1) / span.Ticks * span.Ticks);
        public static DateTimeOffset Round(this DateTimeOffset date, TimeSpan span) => new DateTimeOffset((date.Ticks + (span.Ticks / 2) + 1) / span.Ticks * span.Ticks, date.Offset);

        public static DateTime Ceiling(this DateTime date, TimeSpan span) => new DateTime((date.Ticks + span.Ticks - 1) / span.Ticks * span.Ticks);
        public static DateTimeOffset Ceiling(this DateTimeOffset date, TimeSpan span) => new DateTimeOffset((date.Ticks + span.Ticks - 1) / span.Ticks * span.Ticks, date.Offset);

        public static DateTimeOffset ToTimeZone(this DateTimeOffset date, TimeZoneInfo destinationTimeZone) => TimeZoneInfo.ConvertTime(date, destinationTimeZone);
    }
}
