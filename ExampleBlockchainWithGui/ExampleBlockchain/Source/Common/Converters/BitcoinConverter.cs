using System.Linq;
using BlockchainApp.Source.Common.Extensions;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using MoreLinq;
using static BlockchainApp.Source.Common.Utils.CryptoUtils;
using BlockchainApp.Source.Common.Extensions.Collections;

namespace BlockchainApp.Source.Common.Converters
{
    public static class BitcoinConverter
    {
        // AsymmetricKeyParameter (EC Private Key) --> byte[] (Bitcoin Compressed Private Key)
        public static byte[] ToBitcoinCompressedPrivateKey(this AsymmetricKeyParameter ecPrivateKey)
        {
            var rawPrivKey = ecPrivateKey.ToECPrivateKeyByteArray();
            return ECPrivateKeyByteArrayToBitcoinCompressedPrivateKey(rawPrivKey);
        }

        // byte[] (EC Private Key) --> byte[] (Bitcoin Compressed Private Key)
        public static byte[] ECPrivateKeyByteArrayToBitcoinCompressedPrivateKey(this byte[] rawPrivateKey)
        {
            var arrPrivateKeyWithVersion = new byte[] { 0x80 }.Concat(rawPrivateKey, new byte[] { 0x01 });
            return arrPrivateKeyWithVersion.Concat(Sha256(Sha256(arrPrivateKeyWithVersion)).Take(4)).ToArray();
        }

        // AsymmetricKeyParameter (EC Public Key) --> byte[] (Bitcoin Compressed Public Key)
        public static byte[] ToBitcoinCompressedPublicKey(this AsymmetricKeyParameter ecPublicKey)
        {
            var publicKey = ((ECPublicKeyParameters)ecPublicKey).Q;
            var xs = publicKey.AffineXCoord.ToBigInteger().ToByteArrayUnsigned().PadStart(32);
            var ys = publicKey.AffineYCoord.ToBigInteger();
            return new[] { (byte)(ys.IsEven() ? 0x02 : 0x03) }.Concat(xs).ToArray();
        }

        // AsymmetricKeyParameter (EC Public Key) --> byte[] (Bitcoin Compressed Address)
        public static byte[] ToBitcoinCompressedAddress(this AsymmetricKeyParameter ecPublicKey)
        {
            var pubKey = ToBitcoinCompressedPublicKey(ecPublicKey);
            var ripemd = new byte[] { 0x00 }.Concat(Ripemd160(Sha256(pubKey))).ToArray();
            return ripemd.Concat(Sha256(Sha256(ripemd)).Take(4)).ToArray();
        }

        // AsymmetricKeyParameter (EC Public Key) --> byte[] (Bitcoin Uncompressed Address)
        public static byte[] ToBitcoinUncompressedAddress(this AsymmetricKeyParameter ecPublicKey)
        {
            var pubKey = ToBitcoinUncompressedPublicKey(ecPublicKey);
            var ripemd = new byte[] { 0x00 }.Concat(Ripemd160(Sha256(pubKey))).ToArray();
            return ripemd.Concat(Sha256(Sha256(ripemd)).Take(4)).ToArray();
        }

        // byte[] (Bitcoin Compressed Private Key) --> ECPrivateKeyParameters (EC Private Key)
        public static ECPrivateKeyParameters BitcoinCompressedPrivateKeyToECPrivateKey(this byte[] bPriv)
        {
            var privKey = bPriv.Skip(1).SkipLast(5).ToArray();
            var curve = ECNamedCurveTable.GetByName("secp256k1");
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
            return new ECPrivateKeyParameters(privKey.ToBigIntU(), domainParams);
        }

        // byte[] (Bitcoin Compressed Private Key) --> byte[] (EC Private Key)
        public static byte[] BitcoinCompressedPrivateKeyToECPrivateKeyByteArray(this byte[] bPriv)
        {
            return bPriv.BitcoinCompressedPrivateKeyToECPrivateKey().ToECPrivateKeyByteArray();
        }

        // byte[] (Bitcoin Compressed Public Key) --> ECPublicKeyParameters (EC Public Key)
        public static ECPublicKeyParameters BitcoinCompressedPublicKeyToECPublicKey(this byte[] bPubC)
        {
            var pubKey = bPubC.Skip(1).ToArray();

            var curve = ECNamedCurveTable.GetByName("secp256k1");
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

            var yParity = bPubC.Take(1).ToBigInt().Subtract(BigInteger.Two);
            var x = pubKey.ToBigIntU();
            var p = ((FpCurve)curve.Curve).Q;
            var a = x.ModPow(3.ToBigInt(), p).Add(7.ToBigInt()).Mod(p);
            var y = a.ModPow(p.Add(1.ToBigInt()).FloorDivide(4.ToBigInt()), p);

            if (!y.Mod(BigInteger.Two).Equals(yParity))
                y = y.Negate().Mod(p);

            var q = curve.Curve.CreatePoint(x, y);
            return new ECPublicKeyParameters(q, domainParams);
        }

        // byte[] (Bitcoin Compressed Public Key) --> byte[] (Bitcoin Uncompressed Public Key)
        public static byte[] BitcoinDecompressPublicKey(this byte[] bPubC)
        {
            var ecPubKey = BitcoinCompressedPublicKeyToECPublicKey(bPubC);
            return ToBitcoinUncompressedPublicKey(ecPubKey);
        }

        // byte[] (Bitcoin Uncompressed Public Key) --> byte[] (Bitcoin Compressed Public Key)
        public static byte[] BitcoinCompressPublicKey(this byte[] bPubU)
        {
            var ecPubKey = BitcoinUncompressedPublicKeyToECPublicKey(bPubU);
            return ToBitcoinCompressedPublicKey(ecPubKey);
        }

        // byte[] (Bitcoin Uncompressed Public Key) --> AsymmetricKeyParameter (EC Public Key)
        private static AsymmetricKeyParameter BitcoinUncompressedPublicKeyToECPublicKey(this byte[] bPubU)
        {
            var publicKey = bPubU.Skip(1).ToArray();
            var xs = publicKey.Take(publicKey.Length / 2).ToArray();
            var ys = publicKey.Skip(publicKey.Length / 2).ToArray();

            var curve = ECNamedCurveTable.GetByName("secp256k1");
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

            var x = xs.ToBigIntU();
            var y = ys.ToBigIntU();

            var q = curve.Curve.CreatePoint(x, y);
            return new ECPublicKeyParameters(q, domainParams);
        }

        // byte[] (Bitcoin Compressed Public Key) --> byte[] (EC Public Key)
        public static byte[] BitcoinCompressedPublicKeyToECPublicKeyByteArray(this byte[] bPubC)
        {
            return bPubC.BitcoinDecompressPublicKey().Skip(1).ToArray();
        }

        // byte[] (EC Private Key) --> this byte[] (Bitcoin Uncompressed Private Key)
        public static byte[] ToBitcoinUncompressedPrivateKey(this AsymmetricKeyParameter ecPrivateKey)
        {
            var rawPrivKey = ecPrivateKey.ToECPrivateKeyByteArray();
            return ECPrivateKeyByteArrayToBitcoinUncompressedPrivateKey(rawPrivKey);
        }

        // byte[] (EC Public Key) --> this byte[] (Bitcoin Uncompressed Public Key)
        public static byte[] ToBitcoinUncompressedPublicKey(this AsymmetricKeyParameter ecPublicKey)
        {
            var publicKey = ((ECPublicKeyParameters)ecPublicKey).Q;
            var xs = publicKey.AffineXCoord.ToBigInteger().ToByteArrayUnsigned().PadStart(32);
            var ys = publicKey.AffineYCoord.ToBigInteger().ToByteArrayUnsigned().PadStart(32);
            return new byte[] { 0x04 }.ConcatMany(xs, ys).ToArray();
        }

        // byte[] (EC Private Key) --> this byte[] (Bitcoin Uncompressed Private Key)
        public static byte[] ECPrivateKeyByteArrayToBitcoinUncompressedPrivateKey(this byte[] ecPrivateKey)
        {
            var arrPrivateKeyWithVersion = new byte[] { 0x80 }.Concat(ecPrivateKey);
            return arrPrivateKeyWithVersion.Concat(Sha256(Sha256(arrPrivateKeyWithVersion)).Take(4)).ToArray();
        }

        // byte[] (Bitcoin Compressed Public Key) --> this byte[] (Bitcoin Address)
        public static byte[] BitcoinCompressedPublicKeyToAddress(byte[] bPubC)
        {
            return bPubC.BitcoinCompressedPublicKeyToECPublicKey().ToBitcoinCompressedAddress();
        }
    }
}
