using System.Linq;
using BlockchainApp.Source.Common.Extensions;
using MoreLinq;
using static BlockchainApp.Source.Common.Utils.CryptoUtils;

namespace BlockchainApp.Source.Common.Utils
{
    public static class BitcoinUtils
    {
        public static bool IsCorrectBitcoinAddress(this string address) => address.IsValidAsCompressed(25);
        public static bool IsCorrectBitcoinCompressedPrivateKey(this string privKey) => privKey.IsValidAsCompressed(38);

        private static bool IsValidAsCompressed(this string str, int length)
        {
            if (str.IsNullOrEmpty() || !str.IsBase58())
                return false;

            var arr = str.ToBase58ByteArray();
            if (arr.Length != length)
                return false;

            var beforeCheckSum = arr.SkipLast(4).ToArray();
            var expectedCheckSum = str.ToBase58ByteArray().TakeLast(4).ToArray();
            var calculatedCheckSUm = Sha256(Sha256(beforeCheckSum)).Take(4).ToArray();
            return expectedCheckSum.SequenceEqual(calculatedCheckSUm);
        }
    }
}
