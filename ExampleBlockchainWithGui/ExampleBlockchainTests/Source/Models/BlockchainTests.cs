using System.Collections.Generic;
using System.Linq;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using BlockchainApp.Source.Models;
using BlockchainApp.Source.Models.Wallets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static BlockchainApp.Source.Common.Utils.CryptoUtils;

namespace BlockchainTests.Source.Models
{
    [TestClass]
    public class BlockchainTests
    {
        private Blockchain _blockchain, _newBlockchain;
        private List<Block> _originalChain;

        [TestInitialize]
        public void InitTestBlockchain()
        {
            _blockchain = new Blockchain();
            _originalChain = _blockchain.Chain;
            _newBlockchain = new Blockchain();
        }

        [TestMethod]
        [Description("contains a `chain` List instance")]
        public void BlockchainTest_ContainsProperChainInstance()
        {
            Assert.IsInstanceOfType(_blockchain.Chain, typeof(List<Block>));
        }

        [TestMethod]
        [Description("starts with the genesis block")]
        public void BlockchainTest_StartWithGenesisBlock()
        {
            Assert.AreEqual(_blockchain.Chain.First(), Block.Genesis());
        }

        [TestMethod]
        [Description("adds a new block")]
        public void BlockchainTest_AddNewBlock()
        {
            const string newData = "test-data";
            _blockchain.MineBlock(new Dictionary<string, Transaction> { [newData] = Transaction.Empty() });
            Assert.AreEqual(_blockchain.Chain.Last().Transactions.Single().Key, newData);
        }


        private static Blockchain InitForIsChainValidAndReplaceChainTests(Blockchain bc = null)
        {
            var blockchain = bc ?? new Blockchain();
            blockchain.MineBlock(new Dictionary<string, Transaction> { ["Bears"] = Transaction.Empty() });
            blockchain.MineBlock(new Dictionary<string, Transaction> { ["Foxes"] = Transaction.Empty() });
            blockchain.MineBlock(new Dictionary<string, Transaction> { ["Racoons"] = Transaction.Empty() });
            return blockchain;
        }

        [TestMethod]
        [Description("invalidates a chain with a corrupt genesis block")]
        public void BlockchainTest_InvalidateChainWithCorruptGenesisBlock()
        {
            var blockchain = InitForIsChainValidAndReplaceChainTests();
            blockchain.Chain[0].Transactions = new Dictionary<string, Transaction> { ["Bad data"] = Transaction.Empty() };
            Assert.IsFalse(blockchain.IsChainValid());
        }

        [TestMethod]
        [Description("validates a valid chain")]
        public void BlockchainTest_ValidateValidChain()
        {
            var blockchain = InitForIsChainValidAndReplaceChainTests();
            Assert.IsTrue(blockchain.IsChainValid());
        }

        [TestMethod]
        [Description("invalidates a corrupt chain")]
        public void BlockchainTest_InvalidateCorruptChain()
        {
            var blockchain = InitForIsChainValidAndReplaceChainTests();
            blockchain.Chain[1].Transactions = new Dictionary<string, Transaction> { ["Bad test block"] = Transaction.Empty() };
            Assert.IsFalse(blockchain.IsChainValid());
        }

        [TestMethod]
        [Description("'lastHash' reference has changed")]
        public void BlockchainTest_LastHashChanged()
        {
            var blockchain = InitForIsChainValidAndReplaceChainTests();
            blockchain.Chain[2].LastHash = "Bad-lastHash";
            Assert.IsFalse(blockchain.IsChainValid());
        }

        [TestMethod]
        [Description("the chain contains a block with a jumped difficulty")]
        public void BlockchainTest_ChainContainsBLockWithJumpedDifficulty()
        {
            var blockchain = InitForIsChainValidAndReplaceChainTests();

            var lastBlock = blockchain.Chain.Last();
            var lastHash = lastBlock.Hash;
            var timeStamp = UnixTimestamp.UtcNow();
            const int nonce = 0;
            var data = new Dictionary<string, Transaction>();
            var difficulty = lastBlock.Difficulty - 3;
            var hash = Sha256((timeStamp.ToNoCommaAccurateString(15) + lastHash + data.JsonSerialize() + nonce + difficulty).ToUTF8ByteArray()).ToHexString();
            var badBlock = new Block(timeStamp, lastHash, hash, data, nonce, difficulty);
            blockchain.Chain.Add(badBlock);

            Assert.IsFalse(blockchain.IsChainValid());

        }

        [TestMethod]
        [Description("does not replace the chain when new chain is not longer")]
        public void BlockchainTest_DoNotReplaceChainWithShorterChain()
        {
            _newBlockchain.Chain[0].Transactions = new Dictionary<string, Transaction> { ["other-data"] = Transaction.Empty() };
            _blockchain.ReplaceChain(_newBlockchain.Chain);
            CollectionAssert.AreEqual(_blockchain.Chain, _originalChain);
        }

        [TestMethod]
        [Description("does not replace the chain if new chain is invalid")]
        public void BlockchainTest_DoNotReplaceChainWithInvalidChain()
        {
            var newBlockchain = InitForIsChainValidAndReplaceChainTests();
            newBlockchain.Chain[2].Hash = "some-fake-hash";
            _blockchain.ReplaceChain(newBlockchain.Chain);
            CollectionAssert.AreEqual(_blockchain.Chain, _originalChain);
        }

        [TestMethod]
        [Description("replaces the chain with a valid chain")]
        public void BlockchainTest_ReplaceChainWithValidChain()
        {
            var newBlockchain = InitForIsChainValidAndReplaceChainTests();
            _blockchain.ReplaceChain(newBlockchain.Chain);
            CollectionAssert.AreEqual(_blockchain.Chain, newBlockchain.Chain);
        }

        [TestMethod]
        [Description("validates transactions if the flag is true")]
        public void BlockchainTest_ValidateTransactionsIfFlagIsTrue()
        {
            var mock = new Mock<Blockchain> { CallBase = true };
            var blockchain = mock.Object.SetBlocks(_blockchain.Chain);
            var newBlockchain = InitForIsChainValidAndReplaceChainTests();
            newBlockchain.MineBlock(new Dictionary<string, Transaction> { ["new-test-block"] = Transaction.Empty() });

            mock.Setup(m => m.IsTransactionDataValid(It.IsAny<List<Block>>()));

            blockchain.ReplaceChain(newBlockchain.Chain, true); //

            mock.Verify(m => m.IsTransactionDataValid(newBlockchain.Chain), Times.Once);
        }


        private class ForTransactionDataTests
        {
            public CustomWallet Wallet { get; set; }
            public Transaction Transaction { get; set; }
            public Transaction RewardTransaction { get; set; }
            public Transaction RewardTransaction2 { get; set; }
        }

        private ForTransactionDataTests InitForTransactionDataTests()
        {
            var ftdt = new ForTransactionDataTests { Wallet = new CustomWallet().Create() };
            ftdt.Transaction = new Transaction().CreateAsValid(ftdt.Wallet, "test-address", 65);
            ftdt.RewardTransaction = Transaction.Reward(ftdt.Wallet);
            ftdt.RewardTransaction2 = Transaction.Reward(ftdt.Wallet);
            return ftdt;
        }

        [TestMethod]
        [Description("transaction data is valid")]
        public void BlockchainTest_TransactionDataIsValid()
        {
            var ftdt = InitForTransactionDataTests();
            _newBlockchain.MineBlock(new Dictionary<string, Transaction>
            {
                [ftdt.Transaction.Id] = ftdt.Transaction,
                [ftdt.RewardTransaction.Id] = ftdt.RewardTransaction
            });
            Assert.IsTrue(_blockchain.IsTransactionDataValid(_newBlockchain.Chain));
        }

        [TestMethod]
        [Description("transaction data cannot have multiple rewards")]
        public void BlockchainTest_TransactionDataCantHaveMultipleRewards()
        {
            var ftdt = InitForTransactionDataTests();
            _newBlockchain.MineBlock(new Dictionary<string, Transaction>
            {
                [ftdt.Transaction.Id] = ftdt.Transaction,
                [ftdt.RewardTransaction.Id] = ftdt.RewardTransaction,
                [ftdt.RewardTransaction2.Id] = ftdt.RewardTransaction2
            });
            Assert.IsFalse(_blockchain.IsTransactionDataValid(_newBlockchain.Chain));
        }

        [TestMethod]
        [Description("transaction cant have malformed outputs")]
        public void BlockchainTest_TransactionOutputHasInvalidAmount()
        {
            var ftdt = InitForTransactionDataTests();
            ftdt.Transaction.Outputs[ftdt.Wallet.Address].Amount = 999999;
            _newBlockchain.MineBlock(new Dictionary<string, Transaction>
            {
                [ftdt.Transaction.Id] = ftdt.Transaction,
                [ftdt.RewardTransaction.Id] = ftdt.RewardTransaction
            });
            Assert.IsFalse(_blockchain.IsTransactionDataValid(_newBlockchain.Chain));
        }

        [TestMethod]
        [Description("reward transaction cant have malformed outputs")]
        public void BlockchainTest_RewardTransactionOutputHasInvalidAmount()
        {
            var ftdt = InitForTransactionDataTests();
            ftdt.RewardTransaction.Outputs[ftdt.Wallet.Address].Amount = 999999;
            _newBlockchain.MineBlock(new Dictionary<string, Transaction>
            {
                [ftdt.Transaction.Id] = ftdt.Transaction,
                [ftdt.RewardTransaction.Id] = ftdt.RewardTransaction
            });
            Assert.IsFalse(_blockchain.IsTransactionDataValid(_newBlockchain.Chain));
        }

        [TestMethod]
        [Description("transaction can't have malformed input")]
        public void BlockchainTest_TransactionInputIsMalformed()
        {
            var ftdt = InitForTransactionDataTests();

            ftdt.Wallet.Balance = 9000;

            var badOutputs = new Dictionary<string, TransactionOutput>
            {
                ["8900"] = new TransactionOutput
                {
                    Address = "8900",
                    Amount = 100
                }
            };

            var badTransaction = new Transaction
            {
                Id = UniqueId(),
                Input = new TransactionInput
                {
                    TimeStamp = UnixTimestamp.UtcNow(),
                    Amount = ftdt.Wallet.Balance,
                    Address = ftdt.Wallet.Address
                },
                Outputs = badOutputs
            };
            badTransaction.Input.Signature = ftdt.Wallet.Sign(badTransaction.Hash());

            _newBlockchain.MineBlock(new Dictionary<string, Transaction>
            {
                [badTransaction.Id] = badTransaction,
                [ftdt.RewardTransaction.Id] = ftdt.RewardTransaction
            });
            Assert.IsFalse(_blockchain.IsTransactionDataValid(_newBlockchain.Chain));
        }
    }
}