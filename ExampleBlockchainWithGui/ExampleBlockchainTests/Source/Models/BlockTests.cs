using System.Collections.Generic;
using System.Linq;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using BlockchainApp.Source.Models;
using BlockchainApp.Source.Models.Wallets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static BlockchainApp.Source.Common.Utils.CryptoUtils;
using static BlockchainApp.Source.Config;

namespace BlockchainTests.Source.Models
{
    [TestClass]
    public class BlockTests
    {
        private static readonly UnixTimestamp _timeStamp = new UnixTimestamp(2000);
        private static readonly string _lastHash = "test-last-hash";
        private static readonly string _hash = "test-hash";
        private static readonly Dictionary<string, Transaction> _data = new[] { "blockchain", "data" }.ToDictionary(d => d, d => Transaction.Empty());
        private static readonly int _nonce = 1;
        private static readonly int _difficulty = 1;
        private static readonly Block _block = new Block(_timeStamp, _lastHash, _hash, _data, _nonce, _difficulty);

        [TestMethod]
        [Description("has a timestamp, lastHash, hash, and data property")]
        public void BlockTest_HasValidProperties()
        {
            Assert.AreEqual(_block.TimeStamp, _timeStamp);
            Assert.AreEqual(_block.LastHash, _lastHash);
            Assert.AreEqual(_block.Hash, _hash);
            CollectionAssert.AreEqual(_block.Transactions, _data);
            Assert.AreEqual(_block.Nonce, _nonce);
            Assert.AreEqual(_block.Difficulty, _difficulty);
        }

        private static Block InitForGenesisBlockTests() => Block.Genesis();

        [TestMethod]
        [Description("genesis block returns a Block instance")]
        public void BlockTest_GenesisBlockReturnsBlockInstance()
        {
            var genesisBlock = InitForGenesisBlockTests();
            Assert.AreEqual(genesisBlock.GetType(), typeof(Block));
        }

        [TestMethod]
        [Description("genesis block returns the genesis data")]
        public void BlockTest_GenesisBlockReturnsGenesisData()
        {
            var genesisBlock = InitForGenesisBlockTests();
            Assert.AreEqual(genesisBlock, GENESIS_DATA);
        }


        private class ForMineBlockTests
        {
            public Block LastBlock { get; set; }
            public Dictionary<string, Transaction> Data { get; set; }
            public Block MinedBlock { get; set; }
        }

        private static ForMineBlockTests InitForMineBlockTests()
        {
            var fmbt = new ForMineBlockTests
            {
 
                LastBlock = Block.Genesis(),
                Data = new Dictionary<string, Transaction> { ["mined data"] = Transaction.Empty() }
            };
            fmbt.MinedBlock = fmbt.LastBlock.Mine(fmbt.Data);
            return fmbt;
        }

        [TestMethod]
        [Description("mine block returns a Block instance")]
        public void BlockTest_MineBlockReturnsBlockInstance()
        {
            var minedBlock = InitForMineBlockTests().MinedBlock;
            Assert.AreEqual(minedBlock.GetType(), typeof(Block));
        }

        [TestMethod]
        [Description("sets the `lastHash` to be the `hash` of the lastBlock")]
        public void BlockTest_LastHashMatchLastBlockHash()
        {
            var fmbt = InitForMineBlockTests();
            Assert.AreEqual(fmbt.MinedBlock.LastHash, fmbt.LastBlock.Hash);
        }

        [TestMethod]
        [Description("sets the `data`")]
        public void BlockTest_SetData()
        {
            var fmbt = InitForMineBlockTests();
            CollectionAssert.AreEqual(fmbt.MinedBlock.Transactions, fmbt.Data);
        }

        [TestMethod]
        [Description("sets timestamp")]
        public void BlockTest_SetTimeStamp()
        {
            var fmbt = InitForMineBlockTests();
            Assert.AreNotEqual(fmbt.MinedBlock.TimeStamp, null);
        }

        [TestMethod]
        [Description("creates a SHA-256 `hash` based on the proper inputs")]
        public void BlockTest_CreateSha256HashBasedOnProperInputs()
        {
            var fmbt = InitForMineBlockTests();
            Assert.AreEqual(fmbt.MinedBlock.Hash,
                Sha256((
                    fmbt.MinedBlock.TimeStamp.ToNoCommaAccurateString(15) + 
                    fmbt.MinedBlock.LastHash + 
                    fmbt.MinedBlock.Transactions.JsonSerialize() + 
                    fmbt.MinedBlock.Nonce + 
                    fmbt.MinedBlock.Difficulty).ToUTF8ByteArray()).ToHexString());
        }

        [TestMethod]
        [Description("generates a hash that matches the difficulty")]
        public void BlockTest_GenerateHashMatchingDifficulty()
        {
            var minedBlock = InitForMineBlockTests().MinedBlock;
            Assert.AreEqual(minedBlock.Hash.Take(minedBlock.Difficulty), "0".Repeat(minedBlock.Difficulty));
        }

        [TestMethod]
        [Description("adjusts the difficulty")]
        public void BlockTest_AdjustDifficulty()
        {
            var fmbt = InitForMineBlockTests();
            int[] possibleResults = { fmbt.LastBlock.Difficulty + 1, fmbt.LastBlock.Difficulty - 1 };

            Assert.IsTrue(fmbt.MinedBlock.Difficulty.In(possibleResults));
        }

        [TestMethod]
        [Description("lowers the difficulty for slowly mined blocks")]
        public void BlockTest_LowersDifficultyForSlowlyMinedBLocks()
        {
            _block.Difficulty = 100;
            var expectedDifficulty = _block.Difficulty - 1;
            Assert.AreEqual(_block.AdjustDifficulty(_block.TimeStamp + MINE_RATE + 100), expectedDifficulty);
        }

        [TestMethod]
        [Description("raises the difficulty for quickly mined blocks")]
        public void BlockTest_RaisesDifficultyForQuicklyMinedBlocks()
        {
            _block.Difficulty = 100;
            var expectedDifficulty = _block.Difficulty + 1;
            Assert.AreEqual(_block.AdjustDifficulty(_block.TimeStamp + MINE_RATE - 100), expectedDifficulty);
        }

        [TestMethod]
        [Description("difficulty has a lower limit of 1")]
        public void BlockTest_DifficultyRespectsLowerLimit()
        {
            _block.Difficulty = -1;
            Assert.AreEqual(_block.AdjustDifficulty(_block.TimeStamp), 1);
        }
    }
}