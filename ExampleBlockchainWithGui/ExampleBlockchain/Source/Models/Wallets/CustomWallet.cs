using System.Diagnostics;
using System.Threading.Tasks;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils;
using Org.BouncyCastle.Crypto;
using BlockchainApp.Source.Common.Converters;
using BlockchainApp.Source.Servers;
using static BlockchainApp.Source.Common.Utils.CryptoUtils;
using static BlockchainApp.Source.Common.Utils.LockUtils;
using static BlockchainApp.Source.Config;

namespace BlockchainApp.Source.Models.Wallets
{
    public class CustomWallet : Wallet
    {
        public decimal SpendableBalance { get; set; }
        
        public CustomWallet Create()
        {
            Balance = STARTING_BALANCE;

            ECKeyPair = GenerateECKeyPair();
            RawECPrivateKey = ECKeyPair.Private.ToECPrivateKeyByteArray().ToHexString();
            RawECPublicKey = ECKeyPair.Public.ToECPublicKeyByteArray().ToHexString();
            PrivateKey = ECKeyPair.Private.ToBitcoinCompressedPrivateKey().ToBase58String();
            UncompressedPrivateKey = ECKeyPair.Private.ToBitcoinUncompressedPrivateKey().ToBase58String();
            PublicKey = ECKeyPair.Public.ToBitcoinCompressedPublicKey().ToBase58String();
            UncompressedPublicKey = ECKeyPair.Public.ToBitcoinUncompressedPublicKey().ToBase58String();

            Address = ECKeyPair.Public.ToBitcoinCompressedAddress().ToBase58String();
            UncompressedAddress = ECKeyPair.Public.ToBitcoinUncompressedAddress().ToBase58String();

            return this;
        }

        public CustomWallet Create(CustomWallet wallet)
        {
            Balance = wallet.Balance;

            ECKeyPair = wallet.ECKeyPair;
            RawECPrivateKey = wallet.RawECPrivateKey;
            RawECPublicKey = wallet.RawECPublicKey;
            PrivateKey = wallet.PrivateKey;
            UncompressedPrivateKey = wallet.UncompressedPrivateKey;
            PublicKey = wallet.PublicKey;
            UncompressedPublicKey = wallet.UncompressedPublicKey;

            Address = wallet.Address;
            UncompressedAddress = wallet.UncompressedAddress;

            return this;
        }

        public CustomWallet CreateWithPrivateKeyCompressed(byte[] privateBTCLikeKey)
        {
            var ecPrivKey = privateBTCLikeKey.BitcoinCompressedPrivateKeyToECPrivateKeyByteArray();
            ECKeyPair = CreateECKeyPair(ecPrivKey);
            RawECPrivateKey = ECKeyPair.Private.ToECPrivateKeyByteArray().ToHexString();
            RawECPublicKey = ECKeyPair.Public.ToECPublicKeyByteArray().ToHexString();
            PrivateKey = ECKeyPair.Private.ToBitcoinCompressedPrivateKey().ToBase58String();
            UncompressedPrivateKey = ECKeyPair.Private.ToBitcoinUncompressedPrivateKey().ToBase58String();
            PublicKey = ECKeyPair.Public.ToBitcoinCompressedPublicKey().ToBase58String();
            UncompressedPublicKey = ECKeyPair.Public.ToBitcoinUncompressedPublicKey().ToBase58String();

            Address = ECKeyPair.Public.ToBitcoinCompressedAddress().ToBase58String();
            UncompressedAddress = ECKeyPair.Public.ToBitcoinUncompressedAddress().ToBase58String();

            return this;
        }

        public override string ToString() => "Custom Wallet -" + base.ToString().AfterFirst("-");

        public string Sign(byte[] dataHash)
        {
            return SignECDSA(ECKeyPair.Private, dataHash).ToHexString();
        }

        public Transaction CreateTransaction(string recipient, decimal amount, Blockchain bc = null)
        {
            return new Transaction().Create(this, recipient, amount, bc);
        }

        public Transaction UpdateTransaction(Transaction transaction, string recipient, decimal amount)
        {
            return transaction.Update(this, recipient, amount);
        }

        public static bool IsAddressCorrect(string address) => address.IsCorrectBitcoinAddress();

        public static bool IsPrivateKeyCorrect(string privateKey) => privateKey.IsCorrectBitcoinCompressedPrivateKey();

        public Transaction CreateOrUpdateAndBroadcastTransaction(string recipient, decimal amount, Blockchain bc, TransactionPool tp, P2PServer p2pServer)
        {
            var transaction = tp.ExistingTransaction(Address);
            transaction = transaction != null
                ? UpdateTransaction(transaction, recipient, amount)
                : CreateTransaction(recipient, amount, bc);

            tp.SetTransaction(transaction);
            p2pServer.BroadcastTransaction(transaction);

            return transaction;
        }

        public Transaction CreateOrUpdateAndBroadcastTransaction_TS(string recipient, decimal amount, Blockchain bc, TransactionPool tp, P2PServer p2pServer)
        {
            return Lock(
                new[] { _syncBlockchain, _syncTransactionPool}, 
                new[] { nameof(_syncBlockchain), nameof(_syncTransactionPool) }, 
                nameof(CreateOrUpdateAndBroadcastTransaction_TS), () => 
                    CreateOrUpdateAndBroadcastTransaction(recipient, amount, bc, tp, p2pServer));
        }

        public async Task<Transaction> CreateOrUpdateAndBroadcastTransactionAsync_TS(string recipient, decimal amount, Blockchain bc, TransactionPool tp, P2PServer p2pServer)
        {
            return await Task.Run(() => CreateOrUpdateAndBroadcastTransaction_TS(recipient, amount, bc, tp, p2pServer));
        }

        public virtual decimal UpdateBalance(Blockchain bc)
        {
            var balance = bc.CalculateAddressBalance(Address);
            Balance = balance; 
            return balance;
        }

        public decimal UpdateSpendableBalance(TransactionPool tp)
        {
            var speendableBalance = tp.CalculateAddressSpendableBalance(Address, Balance);
            SpendableBalance = speendableBalance;
            return speendableBalance;
        }

        public CustomWallet UpdateBalances(Blockchain bc, TransactionPool tp)
        {
            UpdateBalance(bc);
            UpdateSpendableBalance(tp);
            return this;
        }

        public static CustomWallet BlockchainWallet()
        {
            var ecKeyPair = CreateECKeyPair("blockchain-wallet".ToUTF8ByteArray());
            return new CustomWallet
            {
                ECKeyPair = ecKeyPair,
                RawECPrivateKey = ecKeyPair.Private.ToECPrivateKeyByteArray().ToHexString(),
                RawECPublicKey = ecKeyPair.Public.ToECPublicKeyByteArray().ToHexString(),
                PrivateKey = ecKeyPair.Private.ToBitcoinCompressedPrivateKey().ToBase58String(),
                UncompressedPrivateKey = ecKeyPair.Private.ToBitcoinUncompressedPrivateKey().ToBase58String(),
                PublicKey = ecKeyPair.Public.ToBitcoinCompressedPublicKey().ToBase58String(),
                UncompressedPublicKey = ecKeyPair.Public.ToBitcoinUncompressedPublicKey().ToBase58String(),

                Address = ecKeyPair.Public.ToBitcoinCompressedAddress().ToBase58String(),
                UncompressedAddress = ecKeyPair.Public.ToBitcoinUncompressedAddress().ToBase58String()
            };
        }


        public decimal UpdateBalance_TS(Blockchain bc)
        {
            return Lock(_syncBlockchain, nameof(_syncBlockchain), nameof(UpdateBalance_TS), () =>
                UpdateBalance(bc));
        }

        public decimal UpdateSpendableBalance_TS(TransactionPool tp)
        {
            return Lock(_syncTransactionPool, nameof(_syncTransactionPool), nameof(UpdateSpendableBalance_TS), () =>
                UpdateSpendableBalance(tp));
        }

        public CustomWallet UpdateBalances_TS(Blockchain bc, TransactionPool tp)
        {
            UpdateBalance_TS(bc);
            UpdateSpendableBalance_TS(tp);
            return this;
        }
    }
}
