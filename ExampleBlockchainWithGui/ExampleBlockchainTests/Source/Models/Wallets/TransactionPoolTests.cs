using System.Collections.Generic;
using System.Linq;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Models;
using BlockchainApp.Source.Models.Wallets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlockchainTests.Source.Models.Wallets
{
    [TestClass]
    public class TransactionPoolTests
    {
        private CustomWallet _senderWallet;
        private TransactionPool _transactionPool;
        private Transaction _transaction;

        [TestInitialize]
        public void InitTestTransactionPools()
        {
            _transactionPool = new TransactionPool();
            _senderWallet = new CustomWallet().Create();
            _transaction = new Transaction().CreateAsValid(_senderWallet, "fake-recipient", 50);
        }

        [TestMethod]
        [Description("adds a transaction to the pool")]
        public void TransactionPoolTest_AddTransactionToPool()
        {
            _transactionPool.SetTransaction(_transaction);
            Assert.AreEqual(_transactionPool.Transactions[_transaction.Id], _transaction);
        }

        [TestMethod]
        [Description("returns an existing transaction given an input address")]
        public void TransactionPoolTest_GetExistingTransactionByInputAddress()
        {
            _transactionPool.SetTransaction(_transaction);
            Assert.AreEqual(_transactionPool.ExistingTransaction(_senderWallet.Address), _transaction);
        }


        private List<Transaction> InitForValidTransactionsTests()
        {
            var validTransactions = new List<Transaction>();

            for (var i = 0; i < 10; i++)
            {
                var transaction = new Transaction().CreateAsValid(_senderWallet, "any-recipient", 30);

                if (i % 3 == 0)
                    transaction.Input.Amount = 999999;
                else if (i % 3 == 1)
                    transaction.Input.Signature = new CustomWallet().Create().Sign("test-data".ToUTF8ByteArray());
                else
                    validTransactions.Add(transaction);

                _transactionPool.SetTransaction(transaction);
            }

            return validTransactions;
        }

        [TestMethod]
        [Description("grabs only valid transactions")]
        public void TransactionPoolTest_GrabOnlyValidTransactions()
        {
            var validTransactions = InitForValidTransactionsTests();
            CollectionAssert.AreEqual(
                _transactionPool.ValidTransactions().Values.OrderBy(t => t.Id).ToArray(),
                validTransactions.OrderBy(t => t.Id).ToArray());
        }

        [TestMethod]
        [Description("clears transaction pool")]
        public void TransactionPoolTest_ClearTransactionPool()
        {
            _transactionPool.Clear();
            CollectionAssert.AreEqual(_transactionPool.Transactions, Enumerable.Empty<Transaction>().ToArray());
        }

        [TestMethod]
        [Description("clears the pool of any existing blockchain transactions")]
        public void TransactionPoolTest_ClearBlockchainTransactions()
        {
            var blockchain = new Blockchain();
            var expectedTransactionMap = new Dictionary<string, Transaction>();

            for (var i = 0; i < 6; i++)
            {
                var transaction = new Transaction().CreateAsValid(new CustomWallet().Create(), "test-transaction", 20);
                _transactionPool.SetTransaction(transaction);

                if (i % 2 == 0)
                    blockchain.MineBlock(new Dictionary<string, Transaction> { [transaction.Id] = transaction });
                else
                    expectedTransactionMap[transaction.Id] = transaction;
            }

            _transactionPool.ClearBlockchainTransactions(blockchain.Chain);

            CollectionAssert.AreEqual(_transactionPool.Transactions, expectedTransactionMap);
        }

        [TestMethod]
        [Description("transactions are properly serialized by any available method")]
        public void TransactionPoolTest_SerializeTransactionsProperly()
        {
            var validTransactions = InitForValidTransactionsTests();
            Assert.AreEqual(validTransactions.ToJToken().ToString(), validTransactions.JsonSerialize());
        }
    }
}
