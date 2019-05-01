using System.Collections.Generic;
using System.Linq;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Models.ViewModels;
using BlockchainApp.Source.Models.Wallets;

namespace BlockchainApp.Source.Common.Converters
{
    public static class TransactionConverter
    {
        // Transaction --> List<TransactionGvVM>
        public static List<TransactionGvVM> ToTransactionsGvVM(this Transaction transaction)
        {
            var transactionsGvVM = new List<TransactionGvVM>();
            foreach (var output in transaction.Outputs.Values)
            {
                if (output.Address == transaction.Input.Address)
                    continue;

                transactionsGvVM.Add(new TransactionGvVM
                {
                    Id = transaction.Id,
                    TransactionType = transaction.Input.Address == Config.REWARD_INPUT_ADDRESS ? TransactionGvVMType.Reward : TransactionGvVMType.Unspecified,

                    TimeStamp = transaction.Input.TimeStamp,
                    From = transaction.Input.Address,
                    Signature = transaction.Input.Signature,

                    To = output.Address,
                    Amount = output.Amount
                });
            }

            return transactionsGvVM;
        }

        // List<Transaction> --> List<TransactionGvVM>
        public static List<TransactionGvVM> ToTransactionsGvVM(this List<Transaction> transactions, string walletAddress, TransactionGvVMState transactionState)
        {
            var transactionsVM = transactions.SelectMany(t => t.ToTransactionsGvVM())
                .Where(t => walletAddress.In(t.From, t.To)).ToList();

            foreach (var transactionVM in transactionsVM)
            {
                transactionVM.TransactionState = transactionState;
                transactionVM.WalletAddress = walletAddress;

                if (transactionVM.TransactionType == TransactionGvVMType.Unspecified) // Reward should be already set at this point
                {
                    transactionVM.TransactionType = walletAddress == transactionVM.From
                        ? TransactionGvVMType.Outgoing
                        : walletAddress == transactionVM.To
                            ? TransactionGvVMType.Incoming
                            : TransactionGvVMType.Unspecified;
                }
            }

            return transactionsVM;
        }
    }
}
