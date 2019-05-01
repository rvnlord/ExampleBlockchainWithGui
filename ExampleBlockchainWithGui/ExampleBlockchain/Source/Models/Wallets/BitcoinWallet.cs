using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlockchainApp.Source.Common.Converters;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using MoreLinq;
using NBitcoin;
using QBitNinja.Client;
using static BlockchainApp.Source.Common.Utils.CryptoUtils;

namespace BlockchainApp.Source.Models.Wallets
{
    public class BitcoinWallet : Wallet
    {
        private static readonly QBitNinjaClient client = new QBitNinjaClient(Network.Main);

        public BitcoinWallet Create()
        {
            return CreateWithECPrivateKeyByteArray(GenerateECKeyPair().Private.ToECPrivateKeyByteArray());
        }

        public BitcoinWallet CreateWithECPrivateKeyByteArray(byte[] arrECPrivateKey)
        {
            ECKeyPair = CreateECKeyPair(arrECPrivateKey);
            var bitcoinPrivateKey = new Key(arrECPrivateKey.PadStart<byte>(32, 0x00).ToArray());
            var bitcoinUncompressedPrivateKey = new Key(arrECPrivateKey.PadStart<byte>(32, 0x00).ToArray(), -1, false);
            PrivateKey = bitcoinPrivateKey.GetBitcoinSecret(Network.Main).ToString();
            RawECPrivateKey = ((byte[])bitcoinPrivateKey.GetType().GetField("vch", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(bitcoinPrivateKey)).ToHexString();
            UncompressedPrivateKey = bitcoinUncompressedPrivateKey.GetBitcoinSecret(Network.Main).ToString();
            PublicKey = bitcoinPrivateKey.PubKey.ToString().ToHexByteArray().ToBase58String();
            RawECPublicKey = PublicKey.ToBase58ByteArray().BitcoinCompressedPublicKeyToECPublicKeyByteArray().ToHexString();
            UncompressedPublicKey = bitcoinUncompressedPrivateKey.PubKey.ToString().ToHexByteArray().ToBase58String();
            Address = bitcoinPrivateKey.PubKey.GetAddress(Network.Main).ToString();
            UncompressedAddress = bitcoinUncompressedPrivateKey.PubKey.GetAddress(Network.Main).ToString();
            return this;
        }

        public decimal UpdateBalance()
        {
            var balanceModel = client.GetBalance(Address, true).Result;
            if (balanceModel.Operations.Count > 0)
            {
                var unspentCoins = new List<Coin>();
                foreach (var operation in balanceModel.Operations)
                    unspentCoins.AddRange(operation.ReceivedCoins.Select(coin => coin as Coin));
                Balance = unspentCoins.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));
            }

            return Balance;
        }

        public override string ToString() => "Bitcoin Wallet -" + base.ToString().AfterFirst("-");
    }
}
