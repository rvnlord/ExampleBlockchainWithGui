using System;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class DecimalExtensions
    {
        public static decimal Round(this decimal m, int decPlaces)
        {
            return Math.Round(m, decPlaces);
        }
    }
}
