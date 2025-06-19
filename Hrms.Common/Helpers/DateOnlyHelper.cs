using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Helpers
{
    public static class DateOnlyHelper
    {
        public static readonly TimeSpan Offset = new(5, 30, 0);

        public static readonly TimeZoneInfo TimeZone = TimeZoneInfo.CreateCustomTimeZone(
            "Kolkata Time",
            Offset,
            "(GMT+5:45) Asia/Kolkata Time",
            "Kolkata Time"
        );

        public static DateOnly Now => DateOnly.FromDateTime(DateTime.UtcNow);

        public static DateOnly ParseDateOrNow(string s)
        {
            bool success = TryParseDate(s, out DateOnly dateOnly);

            return success ? dateOnly : Now;
        }

        public static DateOnly ParseExactOrNow(string s, string format)
        {
            bool success = TryParseExact(s, format, out DateOnly dateOnly);

            return success ? dateOnly : Now;
        }

        public static bool TryParseDate(string s, out DateOnly result)
        {
            return TryParseExact(s, "yyyy-MM-dd", out result);
        }

        public static bool TryParseExact(string s, string format, out DateOnly result)
        {
            bool success = DateOnly.TryParseExact(s, format, out DateOnly dateOnly);

            result = success ? dateOnly : default;

            return success;
        }
        public static int CompareDate(string s1, string s2)
        {
            DateOnly d1 = ParseDateOrNow(s1);
            DateOnly d2 = ParseDateOrNow(s2);

            return (d2.ToDateTime(TimeOnly.MinValue) - d1.ToDateTime(TimeOnly.MinValue)).Days + 1;
        }
    }
}
