using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using BlockchainApp.Source.Common.Converters;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using BlockchainApp.Source.Models.ViewModels;
using BlockchainApp.Source.Models.Wallets;
using static BlockchainApp.Source.Common.Utils.CryptoUtils;
using static BlockchainApp.Source.Config;

namespace BlockchainApp.Source.Models
{
    public class Block
    {
        public UnixTimestamp TimeStamp { get; set; }
        public string Hash { get; set; }
        public string LastHash { get; set; }
        public int Nonce { get; set; }
        public int Difficulty { get; set; }
        public Dictionary<string, Transaction> Transactions { get; set; }

        public Block() { }

        public Block(UnixTimestamp timestamp, string lastHash, string hash, Dictionary<string, Transaction> transactions, int nonce, int? difficulty = null)
        {
            TimeStamp = timestamp;
            Hash = hash;
            LastHash = lastHash;
            Nonce = nonce;
            Difficulty = difficulty ?? INITIAL_DIFFICULTY;
            Transactions = transactions;
        }

        public Block(Block block)
        {
            TimeStamp = new UnixTimestamp(block.TimeStamp.ToDouble());
            LastHash = block.LastHash;
            Hash = block.Hash;
            Transactions = block.Transactions.ToDictionary(kvp => kvp.Key, kvp => new Transaction(kvp.Value));
            Nonce = block.Nonce;
            Difficulty = block.Difficulty;
        }

        public static Block Genesis() => new Block(GENESIS_DATA);

        public Block Mine(Dictionary<string, Transaction> transactions)
        {
            var lastBlock = this;
            if (transactions.Count == 0 || transactions.Values.All(t => t.Input?.Address == REWARD_INPUT_ADDRESS))
                throw new Exception("You can't mine a block if there is no new transaction data dummy!");

            string hash;
            UnixTimestamp timeStamp;
            var lastHash = lastBlock.Hash;
            int difficulty;
            var nonce = 0;

            do
            {
                nonce++;
                timeStamp = UnixTimestamp.UtcNow();
                difficulty = lastBlock.AdjustDifficulty(timeStamp);
                hash = CalculateHash(timeStamp, lastHash, transactions, nonce, difficulty);
                OnGeneratingHash(lastBlock, hash);
            } while (hash.Take(difficulty) != "0".Repeat(difficulty));
            _logger.Debug($"Current block difficulty: {difficulty}");

            return new Block(timeStamp, lastHash, hash, transactions, nonce, difficulty);
        }

        public int AdjustDifficulty(UnixTimestamp currentTime)
        {
            var lastBlock = this;
            var difficulty = lastBlock.Difficulty;
            return currentTime - lastBlock.TimeStamp > TimeSpan.FromMilliseconds(MINE_RATE) 
                ? Math.Max(difficulty - 1, 1) 
                : Math.Max(difficulty + 1, 1);
        }

        public static string CalculateHash(UnixTimestamp timeStamp, string lastHash, Dictionary<string, Transaction> transactions, int nonce, int difficulty)
        {
            var data = transactions.JsonSerialize();
            return Sha256($"{timeStamp.ToNoCommaAccurateString(15)}{lastHash}{data}{nonce}{difficulty}".ToUTF8ByteArray()).ToHexString();
        }

        public string CalculateHash()
        {
            return CalculateHash(TimeStamp, LastHash, Transactions, Nonce, Difficulty);
        }

        public BlockGvVM ToBlockGvVM() => BlockConverter.ToBlockGvVM(this);

        public override string ToString()
        {
            return "Block - \n" +
                $"    TimeStamp  : {TimeStamp.ToNoCommaAccurateString(15)}\n" +
                $"    Last Hash  : {LastHash.Take(10)}\n" +
                $"    Hash       : {Hash.Take(10)}\n" +
                $"    Nonce      : {Nonce}\n" +
                $"    Difficulty : {Difficulty}\n" +
                $"    Data       : {Regex.Replace(Transactions.JsonSerialize(), "\r\n", "\r\n    ")}\n";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Block that))
                return false;
            return TimeStamp == that.TimeStamp && 
                LastHash == that.LastHash && 
                Hash == that.Hash &&
                Transactions.CollectionEqual(that.Transactions) &&
                Nonce == that.Nonce;
        }

        public override int GetHashCode()
        {
            return TimeStamp.GetHashCode() ^ 3 * 
                LastHash.GetHashCode() ^ 5 * 
                Hash.GetHashCode() ^ 7 * 
                Transactions.GetHashCode() ^ 11 *
                Nonce.GetHashCode() ^ 13;
        }

        public event GeneratedHashEventHandler GeneratedHash;
        protected virtual void OnGeneratingHash(GeneratedhashEventArgs e) => GeneratedHash?.Invoke(this, e);
        protected virtual void OnGeneratingHash(Block block, string generatedHash) => OnGeneratingHash(new GeneratedhashEventArgs(block, generatedHash));
    }

    public delegate void GeneratedHashEventHandler(object sender, GeneratedhashEventArgs e);

    public class GeneratedhashEventArgs
    {
        public Block LastBlock { get; }
        public string GeneratedHash { get; }

        public GeneratedhashEventArgs(Block block, string generatedHash)
        {
            LastBlock = block;
            GeneratedHash = generatedHash;
        }
    }
}