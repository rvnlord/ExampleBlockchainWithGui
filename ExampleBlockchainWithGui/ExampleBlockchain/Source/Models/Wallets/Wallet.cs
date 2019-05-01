using BlockchainApp.Source.Common.Utils.UtilClasses;
using Org.BouncyCastle.Crypto;

namespace BlockchainApp.Source.Models.Wallets
{
    public abstract class Wallet : InformationSender
    {
        public decimal Balance { get; set; }
        public string RawECPublicKey { get; set; }
        public string PublicKey { get; set; }
        public string UncompressedPublicKey { get; set; }
        public string RawECPrivateKey { get; set; }
        public string PrivateKey { get; set; }
        public string UncompressedPrivateKey { get; set; }
        public string Address { get; set; }
        public string UncompressedAddress { get; set; }
        public AsymmetricCipherKeyPair ECKeyPair { get; set; }

        public override string ToString()
        {
            return "Wallet - \n" +
                $"    Raw EC Private Key       : {RawECPrivateKey}\n" +
                $"    Raw EC Public Key        : {RawECPublicKey}\n" +
                $"    Private Key              : {PrivateKey}\n" +
                $"    Public Key               : {PublicKey}\n" +
                $"    Address                  : {Address}\n" +
                $"    Uncompressed Private Key : {UncompressedPrivateKey}\n" +
                $"    Uncompressed Public Key  : {UncompressedPublicKey}\n" +
                $"    Uncompressed Address     : {UncompressedAddress}\n" +
                "\n" +
                $"    Balance                  : {Balance}\n";
        }

        public override bool Equals(object o)
        {
            if (o == null) return false;
            var that = (Wallet) o;
            return RawECPrivateKey == that.RawECPrivateKey &&
                PrivateKey == that.PrivateKey &&
                RawECPublicKey == that.RawECPublicKey &&
                PublicKey == that.PublicKey &&
                Address == that.Address;
        }

        public override int GetHashCode()
        {
            return PrivateKey.GetHashCode() ^ 3 *
               PublicKey.GetHashCode() ^ 5 *
               UncompressedPrivateKey.GetHashCode() ^ 7 *
               UncompressedPublicKey.GetHashCode() ^ 11 *
               RawECPrivateKey.GetHashCode() ^ 13 *
               RawECPublicKey.GetHashCode() ^ 17 *
               Address.GetHashCode() ^ 19 *
               UncompressedAddress.GetHashCode() ^ 23;
        }
    }
}
