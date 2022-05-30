using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackApi.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime? GetValueOrDefaultDate(this DateTimeOffset? date) => date?.Date;

        public static long ToReverseTicksMillis(this DateTime datetime)
        {
            return (long)(new DateTime(2305, 07, 13) - datetime).TotalMilliseconds;
        }

        public static int ToReverseTicks(this DateTime datetime)
        {
            return new DateTime(2305, 07, 13).ToEpoch() - datetime.ToEpoch();
        }

        public static DateTime MillisToDateTime(this long epoch)
        {
            var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epochStart.AddMilliseconds(epoch);
        }

        public static DateTime SecondsToDateTime(this int epoch)
        {
            var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epochStart.AddSeconds(epoch);
        }

        /// <summary>
        /// Parses a milliseconds based epoch to a date OR nothing if it's invalid.
        /// You can also prevent converting dates that are in the future, in the case the epoch
        /// might be invalid.
        /// </summary>
        /// <param name="epoch"></param>
        /// <param name="canBeFutureDate"></param>
        /// <returns></returns>
        public static DateTime? MillisToDateTimeOrNullIfNotValid(this long? epoch, bool canBeFutureDate = true)
        {
            if (epoch == default)
                return null;

            try
            {
                var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var date = epochStart.AddMilliseconds(epoch.Value);
                if (!canBeFutureDate && date > DateTime.UtcNow)
                    return null;
                return date;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parses a seconds based epoch to a date OR nothing if it's invalid.
        /// You can also prevent converting dates that are in the future, in the case the epoch
        /// might be invalid.
        /// </summary>
        /// <param name="epoch"></param>
        /// <param name="canBeFutureDate"></param>
        /// <returns></returns>
        public static DateTime? SecondsToDateTimeOrNullIfNotValid(this long? epoch, bool canBeFutureDate = true)
        {
            if (epoch == default)
                return null;

            try
            {
                var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var date = epochStart.AddSeconds(epoch.Value);
                if (!canBeFutureDate && date > DateTime.UtcNow)
                    return null;
                return date;
            }
            catch
            {
                return null;
            }
        }

        public static long ToEpochMillis(this DateTime dateTime)
        {
            var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(dateTime - epochStart).TotalMilliseconds;
        }

        public static int ToEpoch(this DateTime dateTime)
        {
            var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (int)(dateTime - epochStart).TotalSeconds;
        }

        public static DateTime RoundMilliseconds(this DateTime dateTime)
        {
            return (DateTime)RoundMilliseconds((DateTime?)dateTime);
        }

        public static DateTime? RoundMilliseconds(this DateTime? dateTime)
        {
            if (dateTime == null)
                return null;
            var d = (DateTime)dateTime;
            return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
        }

        public static string ToSqlDateTimeString(this DateTime dateTime)
        {
            var sqlDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            if (dateTime == null)
                return DateTime.MinValue.ToString(sqlDateTimeFormat);
            return dateTime.ToString(sqlDateTimeFormat);
        }

        /// <summary>
        /// Gets the previous occurence of day of week.
        /// </summary>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <returns></returns>
        public static DateTime GetPreviousOccurrenceOfDayOfWeek(this DateTime dateTime, DayOfWeek dayOfWeek)
        {
            return dateTime.Date.AddDays(-1 * ((7 + dateTime.DayOfWeek - dayOfWeek) % 7));
        }

        public static bool IsNullOrDefault(this DateTime? datetime)
        {
            if (datetime == null
                || datetime == default(DateTime)
                || datetime == DateTime.MinValue
                || datetime == DateTime.MaxValue)
                return true;

            return false;
        }

        public static DateTime? GetValueOrNull(this DateTime? dateTime)
        {
            if (IsNullOrDefault(dateTime))
                return null;

            return dateTime.Value;
        }

        public static int CalculateMonthsBetween(this DateTime from, DateTime to)
        {
            if (from > to)
                return CalculateMonthsBetween(to, from);

            var monthDiff = Math.Abs((to.Year * 12 + (to.Month - 1)) - (from.Year * 12 + (from.Month - 1)));

            if (from.AddMonths(monthDiff) > to || to.Day < from.Day)
            {
                return monthDiff - 1;
            }

            return monthDiff;
        }

        private enum BinaryDayOfWeek
        {
            Sunday = 1,
            Monday = 2,
            Tuesday = 4,
            Wednesday = 8,
            Thursday = 16,
            Friday = 32,
            Saturday = 64
        }

        public static int GetDayOfWeek(this DateTime time)
            => (int)Enum.Parse<BinaryDayOfWeek>(time.DayOfWeek.ToString());

        private static readonly Dictionary<string, string> TimespanToStringConversions = new Dictionary<string, string>
        {
            { "day(s)", "%d" },
            { "hour(s)", "%h" },
            { "minute(s)", "%m" }
        };

        public static string ToDayPart(this TimeSpan? span)
            => ConvertTimeSpan(span, "%d", "day(s)");

        public static string ToHourPart(this TimeSpan? span)
            => ConvertTimeSpan(span, "%h", "hours(s)");

        public static string ToMinutePart(this TimeSpan? span)
            => ConvertTimeSpan(span, "%m", "minutes(s)");

        public static string ConvertTimeSpan(TimeSpan? span, string conversion, string descriptor)
        {
            if (!span.HasValue)
                return null;

            var convertedSpan = span.Value.ToString(conversion);

            if (convertedSpan == "0")
                return null;

            return $"{convertedSpan} {descriptor}";
        }

        public static string ToReadableString(this TimeSpan? span)
        {
            string timeString = null;

            TimespanToStringConversions.ForEach(t =>
            {
                var convertedSpan = ConvertTimeSpan(span, t.Value, t.Key);

                if (convertedSpan != null)
                    timeString += timeString == null ? convertedSpan : $" {convertedSpan}";
            });

            return timeString;
        }

        public static DateTime StartOfTheDay(this DateTime d)
            => new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);

        public static DateTime EndOfTheDay(this DateTime d)
            => new DateTime(d.Year, d.Month, d.Day, 23, 59, 59);

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek, int weeksAgo = 0)
        {
            var dayDiff = dt.DayOfWeek - startOfWeek;
            int diff = (7 + dayDiff) % 7 + (7 * weeksAgo);
            return dt.AddDays(-1 * diff).Date;
        }

        public static TimeSpan GetDurationUntilTomorrow(this DateTime now)
            => (now - new DateTime(now.Year, now.Month, now.Day).AddDays(1)).Duration();

        public static int GetAge(this DateTime birthday)
        {
            var ageTimeSpan = DateTime.UtcNow - birthday;
            return (int)(ageTimeSpan.TotalDays / 365.25);
        }

        public static int? GetAge(this DateTime? birthday)
            => birthday == null ? null : GetAge(birthday);

        public static List<DateTime> GetWeeks(this DateTime date)
        {
            var weeks = new List<DateTime>() { date };
            void AddWeeks(DateTime currentWeek)
            {
                var nextWeek = currentWeek.AddDays(7);
                if (nextWeek.Date >= DateTime.UtcNow.Date)
                    return;
                weeks.Add(nextWeek);
                AddWeeks(nextWeek);
            }
            AddWeeks(date);
            return weeks.OrderBy(w => w).ToList();
        }

        public static List<DateTime> GetDaysSince(this DateTime fromDate, DateTime? toDate = null)
        {
            toDate ??= DateTime.UtcNow;
            var day = fromDate;
            var days = new List<DateTime>();

            do
            {
                days.Add(day);
                day = day.AddDays(1);
            } while (day.Date < toDate.Value.Date);

            return days;
        }
    }
}
