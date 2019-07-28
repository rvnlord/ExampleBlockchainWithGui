using System;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class DecimalExtensions
    {
        public static decimal Round(this decimal m, int decPlaces)
        {
            return Math.Round(m, decPlaces);
        }

        public static decimal DecimalPlaces(this decimal dec)
        {
            if (dec == Math.Floor(dec))
                return 0;

            var bits = decimal.GetBits(dec);
            var exponent = bits[3] >> 16;
            var result = exponent;
            long lowDecimal = bits[0] | (bits[1] >> 8);
            while (lowDecimal % 10 == 0)
            {
                result--;
                lowDecimal /= 10;
            }

            return result;
        }
    }
}
