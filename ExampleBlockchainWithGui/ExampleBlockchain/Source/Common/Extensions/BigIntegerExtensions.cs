using Org.BouncyCastle.Math;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class BigIntegerExtensions
    {
        public static string ToHexString(this BigInteger number)
        {
            return number.ToString(16);
        }

        public static bool IsEven(this BigInteger number)
        {
            return number.GetLowestSetBit() != 0;
        }

        public static BigInteger FloorDivide(this BigInteger a, BigInteger b)
        {
            if (a.CompareTo(0.ToBigInt()) > 0 ^ b.CompareTo(0.ToBigInt()) < 0 && !a.Mod(b).Equals(0.ToBigInt()))
                return a.Divide(b).Subtract(1.ToBigIntN());

            return a.Divide(b);
        }
    }
}
