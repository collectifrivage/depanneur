using System;

namespace Depanneur.App.Helpers
{
    public static class DateExtensions
    {
        public static readonly TimeZoneInfo DefaultTimezone = TimeZoneInfo.FindSystemTimeZoneById(Startup.DefaultTimezoneName);

        public static DateTime UtcToLocal(this DateTime value)
        {
            return TimeZoneInfo.ConvertTime(value, TimeZoneInfo.Utc, DefaultTimezone);
        }

        public static DateTime StartOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1, 0, 0, 0, value.Kind);
        }
    }
}
