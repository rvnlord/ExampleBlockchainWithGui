using System.Collections.Generic;
using System.Linq;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using static BlockchainApp.Source.Common.Utils.LockUtils;
using static BlockchainApp.Source.Config;

namespace BlockchainApp.Source.Models.Wallets
{
    public class TransactionPool : InformationSender
    {
        public Dictionary<string, Transaction> Transactions { get; set; }

        public TransactionPool()
        {
            Transactions = new Dictionary<string, Transaction>();
        }

        public void SetTransaction(Transaction transaction)
        {
            transaction.ReceiveInfoWith<Transaction>(Transaction_InformationReceived);
            Transactions[transaction.Id] = transaction;
            OnTransactionChanging(GetTransactions(), transaction);
        }

        public void MergeTransactions(Dictionary<string, Transaction> transactions)
        {
            var tpOldCount = Transactions.Count;
            var newNo = 0;
            var updatedNo = 0;
            var invalidNo = 0;

            foreach (var t in transactions.Values)
            {
                if (t.IsValid())
                {
                    if (Transactions.VorN(t.Id) == null)
                        newNo++;
                    else // it will update even if transaction is the same
                        updatedNo++;
                    SetTransaction(t);
                }
                else
                {
                    OnInformationSending($"Transaction '{t.Id}' is invalid and won't be added to the pool");
                    invalidNo++;
                }
            }

            var tpNewCount = Transactions.Count;

            OnInformationSending(newNo + updatedNo + invalidNo == 0 
                ? "Received Transaction Pool doesn't contain any new transactions" 
                : $"Updating Transaction Pool from {tpOldCount} to {tpNewCount} transactions ({newNo} new, {updatedNo} updated, {invalidNo} invalid)");
        }

        public Transaction ExistingTransaction(string inputAddress)
        {
            return Transactions.Values.SingleOrDefault(t => t.Input.Address == inputAddress);
        }

        public Dictionary<string, Transaction> ValidTransactions()
        {
            return Transactions.Values.Where(transaction =>
            {
                var outputTotal = transaction.Outputs.Values.Aggregate(0m, (total, output) => total + output.Amount);
                return transaction.Input.Amount == outputTotal && transaction.IsValid();
            }).ToDictionary(t => t.Id, t => t);
        }

        public Dictionary<string, Transaction> ValidTransactions_TS()
        {
            return Lock(_syncTransactionPool, nameof(_syncTransactionPool), nameof(ValidTransactions_TS), 
                ValidTransactions);
        }

        public void ClearBlockchainTransactions(List<Block> chain)
        {
            for (var i = 1; i < chain.Count; i++)
            {
                var block = chain[i];
                var transactions = block.Transactions.Values;
                foreach (var transaction in transactions)
                    Transactions.RemoveIfExists(transaction.Id);
            }
            OnTransactionChanging(GetTransactions(), null);
        }

        public void Clear()
        {
            Transactions.Clear();
            OnTransactionChanging(GetTransactions(), null);
        }

        public void Clear_TS()
        {
            Lock (_syncTransactionPool, nameof(_syncTransactionPool), nameof(Clear_TS),
                Clear);
        }

        public List<Transaction> GetTransactions() => Transactions.Values.ToList();

        public decimal CalculateAddressSpendableBalance(string address, decimal currentBalance)
        {
            return currentBalance - Transactions.Values
                .Where(t => t.Input.Address == address)
                .SelectMany(t => t.Outputs.Values.Where(o => o.Address != address))
                .Select(o => o.Amount).Sum();
        }

        private void Transaction_InformationReceived(object sender, InformationSentEventArgs e)
        {
            OnInformationSending(e.Information);
        }

        public event TransactionChangedEventHandler TransactionChanged;
        protected virtual void OnTransactionChanging(TransactionChangedEventArgs e) => TransactionChanged?.Invoke(this, e);
        protected virtual void OnTransactionChanging(List<Transaction> transactions, Transaction changedTransaction) => OnTransactionChanging(new TransactionChangedEventArgs(transactions, changedTransaction));
    }

    public delegate void TransactionChangedEventHandler(object sender, TransactionChangedEventArgs e);

    public class TransactionChangedEventArgs
    {
        public Transaction ChangedTransaction { get; }
        public List<Transaction> Transactions { get; }

        public TransactionChangedEventArgs(List<Transaction> transactions, Transaction changedTransaction)
        {
            Transactions = transactions;
            ChangedTransaction = changedTransaction;
        }
    }
}
