using System.Linq;
using BlockchainApp.Source.Common.Extensions;
using MoreLinq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using static BlockchainApp.Source.Common.Utils.CryptoUtils;

namespace BlockchainApp.Source.Common.Converters
{
    public static class CryptographyConverter
    {
        // AsymmetricKeyParameter (EC Private key) --> byte[] (EC Private key)
        public static byte[] ToECPrivateKeyByteArray(this AsymmetricKeyParameter ecPrivateKey)
        {
            return ((ECPrivateKeyParameters)ecPrivateKey).D.ToString(16).ToHexByteArray().PadStart<byte>(32, 0x00).ToArray();
        }

        // AsymmetricKeyParameter (EC Public key) --> byte[] (EC Public key)
        public static byte[] ToECPublicKeyByteArray(this AsymmetricKeyParameter ecPublicKey)
        {
            var publicKey = ((ECPublicKeyParameters)ecPublicKey).Q;
            var xs = publicKey.AffineXCoord.ToBigInteger().ToByteArrayUnsigned().PadStart(32);
            var ys = publicKey.AffineYCoord.ToBigInteger().ToByteArrayUnsigned().PadStart(32);
            return xs.Concat(ys).ToArray();
        }

        // 
        public static byte[] ECPrivateKeyByteArrayToECPublicKeyByteArray(this byte[] ecPrivateKey)
        {
            return CreateECKeyPair(ecPrivateKey).Public.ToECPublicKeyByteArray();
        }
    }
}
