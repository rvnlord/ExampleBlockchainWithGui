using System;
using BlockchainApp.Source.Common.Utils.TypeUtils;
using BlockchainApp.Source.Common.Utils.UtilClasses;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class DoubleExtensions
    {
        public static bool Eq(this double x, double y)
        {
            return Math.Abs(x - y) < Constants.TOLERANCE;
        }

        public static bool Eq(this double? x, double? y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.ToDouble().Eq(y.ToDouble());
        }

        public static bool BetweenExcl(this double d, double greaterThan, double lesserThan)
        {
            TUtils.SwapIf((gt, lt) => gt > lt, ref greaterThan, ref lesserThan);
            return d > greaterThan && d < lesserThan;
        }

        public static bool Between(this double d, double greaterThan, double lesserThan)
        {
            TUtils.SwapIf((gt, lt) => gt > lt, ref greaterThan, ref lesserThan);
            return d >= greaterThan && d <= lesserThan;
        }

        public static double Round(this double number, int digits = 0)
        {
            return Math.Round(number, digits);
        }

        public static int Floor(this double number)
        {
            return (int)Math.Floor(number);
        }

        public static int Ceiling(this double number)
        {
            return (int)Math.Ceiling(number);
        }

        public static double Abs(this double number)
        {
            return Math.Abs(number);
        }


        public static UnixTimestamp ToUnixTimestamp(this double d) => new UnixTimestamp(d);

        public static ExtendedTime ToExtendedTime(this double unixTimestamp, TimeZoneKind timeZone = TimeZoneKind.UTC)
        {
            return new ExtendedTime(unixTimestamp, timeZone);
        }
    }
}
