using System;
using System.Globalization;

namespace Depanneur.App.Models
{
    public class Week
    {
        private static readonly CultureInfo culture = new CultureInfo("fr-CA");
        private DateTime localStart;
        private DateTime localEnd;

        public Week(DateTime localDate)
        {
            localStart = localDate.AddDays(-(int)localDate.DayOfWeek).Date;
            localEnd = localStart.AddDays(7);

            StartUtc = localStart.ToUniversalTime();
            EndUtc = localEnd.ToUniversalTime();
        }

        public DateTime StartUtc { get; }
        public DateTime EndUtc { get; }

        public override string ToString()
        {
            var adjustedEnd = localEnd.AddSeconds(-1);

            var result = localStart.Month == adjustedEnd.Month
                ? (FormattableString) $"du {localStart.Day} au {adjustedEnd:d MMMM yyyy}"
                : $"du {localStart:d MMMM} au {adjustedEnd:d MMMM yyyy}";

            return result.ToString(culture);
        }
    }
}