using System;
using System.Linq;
using System.Text;
using BlockchainApp.Source.Common.Utils;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace BlockchainApp.Source.Common.Extensions.Collections
{
    public static class ArrayExtensions
    {
        #region - Converters

        public static BigInteger ToBigIntU(this byte[] arr)
        {
            return new BigInteger(1, arr);
        }

        public static BigInteger ToBigInt(this byte[] arr)
        {
            return new BigInteger(arr);
        }

        public static string ToUTF8String(this byte[] arr)
        {
            return Encoding.UTF8.GetString(arr);
        }

        public static string ToHexString(this byte[] value, bool prefix = false)
        {
            var strPrex = prefix ? "0x" : "";
            return strPrex + string.Concat(value.Select(b => b.ToString("x2")).ToArray());
        }

        public static string ToBase64String(this byte[] arr)
        {
            return Convert.ToBase64String(arr);
        }

        public static string ToBase58String(this byte[] data, int offset, int count)
        {
            var bn0 = BigInteger.Zero;
            var vchTmp = data.SafeSubarray(offset, count);
            var bn = new BigInteger(1, vchTmp);
            var builder = new StringBuilder();

            while (bn.CompareTo(bn0) > 0)
            {
                var r = bn.DivideAndRemainder(Constants.Bn58);
                var dv = r[0];
                var rem = r[1];
                bn = dv;
                var c = rem.IntValue;
                builder.Append(Constants.PszBase58[c]);
            }

            for (var i = offset; i < offset + count && data[i] == 0; i++)
                builder.Append(Constants.PszBase58[0]);

            var chars = builder.ToString().ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        public static byte[] HexToUTF8(this byte[] arr)
        {
            return arr.ToHexString().ToUTF8ByteArray();
        }

        public static byte[] UTF8ToHex(this byte[] arr)
        {
            return arr.ToUTF8String().ToHexByteArray();
        }

        #endregion

        public static T[] Swap<T>(this T[] a, int i, int j)
        {
            var temp = a[j];
            a[j] = a[i];
            a[i] = temp;
            return a;
        }

        public static bool StartWith(this byte[] data, byte[] versionBytes)
        {
            if (data.Length < versionBytes.Length)
                return false;
            return !versionBytes.Where((t, i) => data[i] != t).Any();
        }

        public static byte[] SafeSubarray(this byte[] array, int offset, int count)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (offset < 0 || offset > array.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (offset == 0 && array.Length == count)
                return array;
            var data = new byte[count];
            Buffer.BlockCopy(array, offset, data, 0, count);
            return data;
        }

        public static byte[] SafeSubarray(this byte[] array, int offset)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (offset < 0 || offset > array.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            var count = array.Length - offset;
            var data = new byte[count];
            Buffer.BlockCopy(array, offset, data, 0, count);
            return data;
        }

        public static byte[] Concat(this byte[] arr, params byte[][] arrs)
        {
            var len = arr.Length + arrs.Sum(a => a.Length);
            var ret = new byte[len];
            Buffer.BlockCopy(arr, 0, ret, 0, arr.Length);
            var pos = arr.Length;
            foreach (var a in arrs)
            {
                Buffer.BlockCopy(a, 0, ret, pos, a.Length);
                pos += a.Length;
            }
            return ret;
        }
    }
}
