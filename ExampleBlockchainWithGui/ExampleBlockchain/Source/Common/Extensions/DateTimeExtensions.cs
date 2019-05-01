using System;
using BlockchainApp.Source.Common.Utils.TypeUtils;
using BlockchainApp.Source.Common.Utils.UtilClasses;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static string MonthName(this DateTime date)
        {
            return Constants.Culture.DateTimeFormat.GetMonthName(date.Month);
        }

        public static DateTime Period(this DateTime date, int periodInDays)
        {
            var startDate = new DateTime();
            var myDate = new DateTime(date.Year, date.Month, date.Day);
            var diff = myDate - startDate;
            return myDate.AddDays(-(diff.TotalDays % periodInDays));
        }

        public static DateTime? ToDMY(this DateTime? dateTimeNullable)
        {
            if (dateTimeNullable == null)
                return null;

            var date = (DateTime)dateTimeNullable;
            date = new DateTime(date.Year, date.Month, date.Day);
            return date;
        }

        public static DateTime ToDMY(this DateTime dateTime)
        {
            var date = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
            return date;
        }

        public static DateTime As(this DateTime dateTime, DateTimeKind kind)
        {
            return DateTime.SpecifyKind(dateTime, kind);
        }

        public static ExtendedTime ToExtendedTime(this DateTime dt, TimeZoneKind tz = TimeZoneKind.UTC)
        {
            return new ExtendedTime(dt, tz);
        }

        public static UnixTimestamp ToUnixTimestamp(this DateTime dateTime)
        {
            return new UnixTimestamp(dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }

        public static bool BetweenExcl(this DateTime d, DateTime greaterThan, DateTime lesserThan)
        {
            TUtils.SwapIf((gt, lt) => gt > lt, ref greaterThan, ref lesserThan);
            return d > greaterThan && d < lesserThan;
        }

        public static bool Between(this DateTime d, DateTime greaterThan, DateTime lesserThan)
        {
            TUtils.SwapIf((gt, lt) => gt > lt, ref greaterThan, ref lesserThan);
            return d >= greaterThan && d <= lesserThan;
        }

        public static DateTime SubtractDays(this DateTime dt, int days)
        {
            return dt.AddDays(-days);
        }

        public static DateTime SubtractYears(this DateTime dt, int years)
        {
            return dt.AddYears(-years);
        }
    }
}
