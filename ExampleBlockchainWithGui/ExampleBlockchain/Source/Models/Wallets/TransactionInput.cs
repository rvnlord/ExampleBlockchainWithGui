using BlockchainApp.Source.Common.Converters;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using static BlockchainApp.Source.Common.Utils.CryptoUtils;

namespace BlockchainApp.Source.Models.Wallets
{
    public class TransactionInput
    {
        public TransactionInput() { }

        public TransactionInput(TransactionInput input)
        {
            TimeStamp = new UnixTimestamp(input.TimeStamp);
            Amount = input.Amount;
            Address = input.Address;
            Signature = input.Signature;
            PublicKey = input.PublicKey;
        }

        public UnixTimestamp TimeStamp { get; set; }
        public decimal Amount { get; set; }
        public string Address { get; set; }
        public string PublicKey { get; set; }
        public string Signature { get; set; }

        public bool Verify(byte[] dataHash)
        {
            return VerifyECDSA(
                PublicKey.ToBase58ByteArray().BitcoinCompressedPublicKeyToECPublicKey(),
                Signature.ToHexByteArray(),
                dataHash);
        }

        public override bool Equals(object o)
        {
            if (!(o is TransactionInput)) return false;
            var that = (TransactionInput)o;
            return TimeStamp == that.TimeStamp &&
                Amount == that.Amount &&
                Address == that.Address &&
                Signature == that.Signature &&
                PublicKey == that.PublicKey;
        }

        public override int GetHashCode()
        {
            return TimeStamp.GetHashCode() ^ 3 *
               Amount.GetHashCode() ^ 5 *
               Address.GetHashCode() ^ 7 *
               Signature.GetHashCode() ^ 11 *
               PublicKey.GetHashCode() ^ 13;
        }
    }
}
