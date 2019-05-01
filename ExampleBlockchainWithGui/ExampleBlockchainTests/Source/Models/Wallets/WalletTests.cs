using System;
using System.Collections.Generic;
using System.Linq;
using BlockchainApp.Source.Common.Converters;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.TypeUtils;
using BlockchainApp.Source.Models;
using BlockchainApp.Source.Models.Wallets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static BlockchainApp.Source.Common.Utils.CryptoUtils;
using static BlockchainApp.Source.Config;

namespace BlockchainTests.Source.Models.Wallets
{
    [TestClass]
    public class WalletTests
    {
        private CustomWallet _wallet;
        private string _data;
        private Blockchain _blockchain;

        [TestInitialize]
        public void InitTestWallets()
        {
            _wallet = new CustomWallet().Create();
            _data = "test-data";
            _blockchain = new Blockchain();
        }

        [TestMethod]
        [Description("validates custom wallet against bitcoin wallet")]
        public void WalletTest_CustomWalletMatchBitcoinWallet()
        {
            var bitcoinWallet = new BitcoinWallet().CreateWithECPrivateKeyByteArray(_wallet.RawECPrivateKey.ToHexByteArray());
            Assert.AreEqual(_wallet, bitcoinWallet);
        }

        [TestMethod]
        [Description("has initial test 'balance'")]
        public void WalletTest_HasInitialBalance()
        {
            Assert.IsTrue(_wallet.Balance > 0);
        }

        [TestMethod]
        [Description("has 'publicKey'")]
        public void WalletTest_HasPublicKey()
        {
            Assert.IsFalse(_wallet.PublicKey.IsNullOrEmpty());
        }

        [TestMethod]
        [Description("has 'address'")]
        public void WalletTest_HasAddress()
        {
            Assert.IsFalse(_wallet.Address.IsNullOrEmpty());
        }

        [TestMethod]
        [Description("verifies signature")]
        public void WalletTest_VerifySignature()
        {
            Assert.IsTrue(VerifyECDSA(
                _wallet.PublicKey.ToBase58ByteArray().BitcoinCompressedPublicKeyToECPublicKey(),
                _wallet.Sign(_data.ToUTF8ByteArray()).ToHexByteArray(),
                _data.ToUTF8ByteArray()));
        }

        [TestMethod]
        [Description("does not verify an invalid signature")]
        public void WalletTest_FailToVerifyInvalidSignature()
        {
            Assert.IsFalse(VerifyECDSA(
                _wallet.PublicKey.ToBase58ByteArray().BitcoinCompressedPublicKeyToECPublicKey(),
                new CustomWallet().Create().Sign(_data.ToUTF8ByteArray()).ToHexByteArray(),
                _data.ToUTF8ByteArray()));
        }

        [TestMethod]
        [Description("throw when creating transaction if amount exceeds the balance")]
        public void WalletTest_ThrowifAmountExceedsBalanceWhenCreatingTransaction()
        {
            var amount = 999999;
            Assertutils.ThrowsExceptionWithMessage(() => _wallet.CreateTransaction("test-recipient", amount), $"Amount: {amount} exceeds balance: {STARTING_BALANCE}");
        }


        private class ForCreateTransactionTests
        {
            public decimal Amount { get; set; }
            public string Recipient { get; set; }
            public Transaction Transaction { get; set; }
        }

        private ForCreateTransactionTests InitForCreateTransactionTests()
        {
            var fct = new ForCreateTransactionTests
            {
                Amount = 50,
                Recipient = "r4nd0m-4ddr355"
            };
            fct.Transaction = new Transaction().CreateAsValid(_wallet, fct.Recipient, fct.Amount);
            new Transaction().CreateAsValid(_wallet, fct.Recipient, fct.Amount);
            return fct;
        }

        [TestMethod]
        [Description("matches the transaction input with the wallet")]
        public void WalletTest_MatchTransactionInputWithWallet()
        {
            var fct = InitForCreateTransactionTests();
            Assert.AreEqual(fct.Transaction.Input.Address, _wallet.Address);
        }

        [TestMethod]
        [Description("outputs the amount to the recipient")]
        public void WalletTest_OutputAmountToRecipient()
        {
            var fct = InitForCreateTransactionTests();
            Assert.AreEqual(fct.Transaction.Outputs[fct.Recipient].Amount, fct.Amount);
        }

        [TestMethod]
        [Description("calls `Wallet.CalculateBalance' during 'CreateTransaction' if chain argument is speciifed")]
        public void WalletTest_CallCalculateBalanceWhenCreatingTransactionIfChainIsSpecified()
        {
            const string recipient = "test-r3cipi3nt";

            var mock = new Mock<CustomWallet> { CallBase = true };
            mock.Setup(m => m.UpdateBalance(_blockchain)).CallBase();
            var wallet = mock.Object.Create(_wallet);

            new Transaction().CreateAsValid(wallet, recipient, 10, _blockchain);

            mock.Verify(m => m.UpdateBalance(_blockchain), Times.Once);
        }

        [TestMethod]
        [Description("'CalculateBalance' returns the `STARTING_BALANCE`")]
        public void WalletTest_CalculateBalanceReturnsInitialBalance()
        {
            Assert.AreEqual(_wallet.UpdateBalance(_blockchain), STARTING_BALANCE);
        }


        private class ForOutputsTests
        {
            public Transaction TransactionOne { get; set; }
            public Transaction TransactionTwo { get; set; }
        }

        private ForOutputsTests InitForOutputsTests()
        {
            var fo = new ForOutputsTests
            {
                TransactionOne = new Transaction().CreateAsValid(new CustomWallet().Create(), _wallet.Address, 50),
                TransactionTwo = new Transaction().CreateAsValid(new CustomWallet().Create(), _wallet.Address, 60)
            };

            _blockchain.MineBlock(new Dictionary<string, Transaction>
            {
                [fo.TransactionOne.Id] = fo.TransactionOne,
                [fo.TransactionTwo.Id] = fo.TransactionTwo
            });
            return fo;
        }

        [TestMethod]
        [Description("adds the sum of all outputs to the wallet balance")]
        public void WalletTest_AddSumOfAllOutputsToWalletBalance()
        {
            var fo = InitForOutputsTests();
            Assert.AreEqual(_wallet.UpdateBalance(_blockchain), 
                STARTING_BALANCE + fo.TransactionOne.Outputs[_wallet.Address].Amount + fo.TransactionTwo.Outputs[_wallet.Address].Amount);
        }


        private Transaction InitForMakeTransactionTests()
        {
            var recentTransaction = new Transaction().CreateAsValid(_wallet, "test-address2", 30);
            _blockchain.MineBlock(new Dictionary<string, Transaction>
            {
                [recentTransaction.Id] = recentTransaction
            });
            return recentTransaction;
        }

        [TestMethod]
        [Description("returns the output amount of the recent transaction")]
        public void WalletTest_ReturnOutputAmountOfRecentTransaction()
        {
            InitForOutputsTests();
            var recentTransaction = InitForMakeTransactionTests();
            Assert.AreEqual(_wallet.UpdateBalance(_blockchain), recentTransaction.Outputs[_wallet.Address].Amount);
        }


        private class ForNextPrevOutputTests
        {
            public Transaction SameBlockTransaction { get; set; }
            public Transaction NextBlockTransaction { get; set; }
            public Transaction RecentTransaction { get; set; }
        }

        private ForNextPrevOutputTests InitForNextPrevOutputTests()
        {
            var fnpo = new ForNextPrevOutputTests
            {
                RecentTransaction = new Transaction().CreateAsValid(_wallet, "later-test-address", 60),
                SameBlockTransaction = Transaction.Reward(_wallet)
            };

            _blockchain.MineBlock(new Dictionary<string, Transaction>
            {
                [fnpo.RecentTransaction.Id] = fnpo.RecentTransaction,
                [fnpo.SameBlockTransaction.Id] = fnpo.SameBlockTransaction
            });

            fnpo.NextBlockTransaction = new CustomWallet().Create().CreateTransaction(_wallet.Address, 75);

            _blockchain.MineBlock(new Dictionary<string, Transaction>
            {
                [fnpo.NextBlockTransaction.Id] = fnpo.NextBlockTransaction
            });

            return fnpo;
        }

        [TestMethod]
        [Description("includes the output amounts in the returned balance")]
        public void WalletTest_IncludeOutputAmountInReturnedBalance()
        {
            InitForOutputsTests();
            InitForMakeTransactionTests();
            var fnpo = InitForNextPrevOutputTests();
            Assert.AreEqual(_wallet.UpdateBalance(_blockchain),
                fnpo.RecentTransaction.Outputs[_wallet.Address].Amount +
                fnpo.SameBlockTransaction.Outputs[_wallet.Address].Amount +
                fnpo.NextBlockTransaction.Outputs[_wallet.Address].Amount);
        }
    }
}
