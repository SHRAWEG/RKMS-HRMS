using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Helpers
{
    public static class DateTimeHelper
    {
        public static readonly TimeSpan Offset = new(5, 30, 0);

        public static readonly TimeZoneInfo TimeZone = TimeZoneInfo.CreateCustomTimeZone(
            "Kolkata Time",
            Offset,
            "(GMT+5:45) Asia/Kolkata Time",
            "Kolkata Time"
        );

        public static DateTimeOffset Now => DateTimeOffset.UtcNow.ToOffset(Offset);

        public static DateTimeOffset SetTime(DateTimeOffset time, int hour, int minute, int second)
        {
            return SetTime(time, hour, minute, second, 0);
        }

        public static DateTimeOffset SetTime(DateTimeOffset time, int hour, int minute, int second, int millisecond)
        {
            return new DateTimeOffset(time.Year, time.Month, time.Day, hour, minute, second, millisecond, time.Offset);
        }

        public static DateTimeOffset SetDayTime(DateTimeOffset time, int day, int hour, int minute, int second, int millisecond)
        {
            return new DateTimeOffset(time.Year, time.Month, day, hour, minute, second, millisecond, time.Offset);
        }

        public static DateTimeOffset FromDateTime(DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToOffset(Offset);
        }

        public static DateTimeOffset ParseDateOrNow(string s)
        {
            bool success = TryParseDate(s, out DateTimeOffset dateTimeOffset);

            return success ? dateTimeOffset : Now;
        }

        public static DateTimeOffset ParseExactOrNow(string s, string format)
        {
            bool success = TryParseExact(s, format, out DateTimeOffset dateTimeOffset);

            return success ? dateTimeOffset : Now;
        }

        public static bool TryParseDate(string s, out DateTimeOffset result)
        {
            return TryParseExact(s, "yyyy-MM-dd", out result);
        }

        public static bool TryParseExact(string s, string format, out DateTimeOffset result)
        {
            bool success = DateTime.TryParseExact(s, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime);

            result = success ? new DateTimeOffset(dateTime, Offset) : default;

            return success;
        }

        public static int CompareDate(string s1, string s2)
        {
            DateTimeOffset d1 = ParseDateOrNow(s1);
            DateTimeOffset d2 = ParseDateOrNow(s2);

            return DateTimeOffset.Compare(d1, d2);
        }

        public static int Compare(string s1, string s2, string format)
        {
            DateTimeOffset d1 = ParseExactOrNow(s1, format);
            DateTimeOffset d2 = ParseExactOrNow(s2, format);

            return DateTimeOffset.Compare(d1, d2);
        }

        public static DateTimeOffset FirstDayOfWeek(DateTimeOffset date)
        {
            DateTimeOffset dt = date;

            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;

            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-diff).Date;
        }
    }
}
