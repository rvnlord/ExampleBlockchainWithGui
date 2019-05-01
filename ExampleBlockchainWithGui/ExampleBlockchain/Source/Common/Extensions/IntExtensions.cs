using System;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class IntExtensions
    {
        #region - Converters

        public static string ToHexString(this int n)
        {
            return n.ToString("X");
        }

        #endregion

        public static T ToEnum<T>(this int n) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an Enum");
            return (T)(object)n;
        }

        public static int Abs(this int n)
        {
            return Math.Abs(n);
        }
    }
}
