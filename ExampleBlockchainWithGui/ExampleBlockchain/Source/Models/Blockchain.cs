using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using BlockchainApp.Source.Models.Wallets;
using static BlockchainApp.Source.Common.Utils.LockUtils;
using static BlockchainApp.Source.Config;

namespace BlockchainApp.Source.Models
{
    public class Blockchain : InformationSender
    {
        public List<Block> Chain { get; set; }

        public Blockchain()
        {
            Chain = new List<Block> { Block.Genesis() };
        }

        public Blockchain(List<Block> blocks)
        {
            Chain = blocks;
        }

        public Block MineBlock(Dictionary<string, Transaction> transactions)
        {
            var lastBlock = Chain.Last();
            lastBlock.RemoveEventHandlers(nameof(lastBlock.GeneratedHash));
            lastBlock.GeneratedHash += LastBlock_Generatedhash;

            var block = Chain.Push(lastBlock.Mine(transactions));
            OnChainChanging(GetTransactions());
            return block;
        }

        public Block MineBlock_TS(Dictionary<string, Transaction> transactions)
        {
            return Lock (_syncBlockchain, nameof(_syncBlockchain), nameof(MineBlock_TS), () =>
                MineBlock(transactions));
        }

        public virtual void ReplaceChain(List<Block> chain, bool validateTransactions = false, Action onSuccess = null)
        {
            if (chain.Count <= Chain.Count)
            {
                OnInformationSending("Received chain is not longer than the current chain.");
                return;
            }

            if (!IsChainValid(chain))
            {
                OnInformationSending("The received chain is not valid.");
                return;
            }

            if (validateTransactions && !IsTransactionDataValid(chain)) {
                OnInformationSending("The incoming chain has invalid data");
                return;
            }

            onSuccess?.Invoke();

            Chain.ReplaceAll(chain);
            OnInformationSending("Replacing blockchain with the new chain.");           
            OnChainChanging(GetTransactions());
        }

        public virtual bool IsTransactionDataValid(List<Block> chain)
        {
            for (var i = 1; i < chain.Count; i++)
            {
                var block = chain[i];
                var rewardTransactionCount = 0;
                var transactions = block.Transactions.Values;

                foreach (var transaction in transactions)
                {
                    if (transaction.Input?.Address == REWARD_INPUT_ADDRESS)
                    {
                        rewardTransactionCount++;

                        if (rewardTransactionCount > 1)
                        {
                            OnInformationSending("Miner rewards exceed limit");
                            return false;
                        }

                        if (transaction.Outputs.Values.Single().Amount != MINING_REWARD)
                        {
                            OnInformationSending("Miner reward amount is invalid");
                            return false;
                        }
                    }
                    else
                    {
                        if (!transaction.IsValid())
                        {
                            OnInformationSending("Invalid transaction");
                            return false;
                        }

                        var balance = CalculateAddressBalanceAtBlock(transaction.Input.Address, chain, i);

                        if (transaction.Input.Amount != balance)
                        {
                            OnInformationSending("Invalid input amount");
                            return false;
                        } // Identical transaction can't appear twice because I am using a dictionary
                    }
                }
            }

            return true;
        }

        public bool IsChainValid(List<Block> chain = null)
        {
            if (chain == null)
                chain = Chain;

            if (!chain.First().Equals(Block.Genesis()))
                return false;

            for (var i = 1; i < chain.Count; i++)
            {
                var block = chain[i];
                var lastBlock = chain[i - 1];

                if (block.LastHash != lastBlock.Hash || block.Hash != block.CalculateHash() || Math.Abs(lastBlock.Difficulty - block.Difficulty) > 1)
                    return false;
            }

            return true;
        }

        public virtual decimal CalculateAddressBalance(string address) => CalculateAddressBalanceAtBlock(address);

        public decimal CalculateAddressBalanceAtBlock(string address, List<Block> blocks = null, int? blockId = null)
        {
            var hasConductedTransaction = false;
            var outputsTotal = 0m;
            var blockNo = blockId ?? Chain.Count;
            var chain = blocks ?? Chain;

            for (var i = blockNo - 1; i > 0; i--)
            {
                var block = chain[i];
                var transactions = block.Transactions.Values;

                foreach (var transaction in transactions)
                {
                    if (transaction.Input.Address == address)
                        hasConductedTransaction = true;

                    var addressOutput = transaction.Outputs.VorN(address);

                    if (addressOutput != null)
                        outputsTotal += addressOutput.Amount;
                }

                if (hasConductedTransaction)
                    break;
            }

            return hasConductedTransaction ? outputsTotal : STARTING_BALANCE + outputsTotal;
        }

        public Blockchain SetBlocks(List<Block> blocks)
        {
            Chain = blocks;
            return this;
        }

        public List<Transaction> GetTransactions() => Chain.SelectMany(b => b.Transactions.Values).ToList();

        private void LastBlock_Generatedhash(object sender, GeneratedhashEventArgs e) => OnGeneratingBlockHash(e); // route forward

        public event ChainChangedEventHandler ChainChanged;
        protected virtual void OnChainChanging(ChainChangedEventArgs e) => ChainChanged?.Invoke(this, e);
        protected void OnChainChanging(List<Transaction> transactions) => OnChainChanging(new ChainChangedEventArgs(transactions));
        public event GeneratedHashEventHandler GeneratedBlockHash;
        protected virtual void OnGeneratingBlockHash(GeneratedhashEventArgs e) => GeneratedBlockHash?.Invoke(this, e);
    }

    public delegate void ChainChangedEventHandler(object sender, ChainChangedEventArgs e);

    public class ChainChangedEventArgs
    {
        public List<Transaction> Transactions { get; }

        public ChainChangedEventArgs(List<Transaction> transactions)
        {
            Transactions = transactions;
        }
    }
}
