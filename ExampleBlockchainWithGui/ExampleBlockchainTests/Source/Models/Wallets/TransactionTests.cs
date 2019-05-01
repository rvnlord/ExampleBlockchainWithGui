using System;
using System.Collections.Generic;
using System.Linq;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.TypeUtils;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using BlockchainApp.Source.Models.Wallets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static BlockchainApp.Source.Common.Utils.CryptoUtils;
using static BlockchainApp.Source.Config;

namespace BlockchainTests.Source.Models.Wallets
{
    [TestClass]
    public class TransactionTests
    {
        private CustomWallet _senderWallet;
        private decimal _amount;
        private string _recipient;
        private Transaction _transaction;

        [TestInitialize]
        public void InitForTransactionTessts()
        {
            _senderWallet = new CustomWallet().Create();
            _recipient = "r3c1p13nt-address";
            _amount = 50;
            _transaction = new Transaction().CreateAsValid(_senderWallet, _recipient, _amount);
        }

        [TestMethod]
        [Description("transaction has an 'id'")]
        public void TransactionTest_HasId()
        {
            Assert.IsFalse(_transaction.Id.IsNullOrEmpty());
        }

        [TestMethod]
        [Description("transaction has 'outputs'")]
        public void TransactionTest_HasOutputs()
        {
            Assert.IsTrue(_transaction.Outputs.Any());
        }

        [TestMethod]
        [Description("outputs the `amount` to the recipient")]
        public void TransactionTest_OutputAmountAddedToRecipient()
        {
            Assert.AreEqual(_transaction.Outputs[_recipient].Amount, _amount);
        }

        [TestMethod]
        [Description("outputs the `amount` subtracted from the wallet balance")]
        public void TransactionTest_OutputAmountSubtractedFromWalletBalance()
        {
            Assert.AreEqual(_transaction.Outputs[_senderWallet.Address].Amount, _senderWallet.Balance - _amount);
        }

        [TestMethod]
        [Description("outputs has an 'input'")]
        public void TransactionTest_HasInput()
        {
            Assert.IsTrue(_transaction.Input != null);
        }

        [TestMethod]
        [Description("has a `timestamp` in the input")]
        public void TransactionTest_InputHasTimeStamp()
        {
            Assert.IsTrue(_transaction.Input?.TimeStamp != null);
        }

        [TestMethod]
        [Description("sets the `amount` to the `senderWallet` balance")]
        public void TransactionTest_InputWalletBalance()
        {
            Assert.AreEqual(_transaction.Input.Amount, _senderWallet.Balance);
        }

        [TestMethod]
        [Description("sets the `address` to the `senderWallet` address")]
        public void TransactionTest_SetAddressToSenderWalletAddress()
        {
            Assert.AreEqual(_transaction.Input.Address, _senderWallet.Address);
        }

        [TestMethod]
        [Description("transaction signs the input")]
        public void TransactionTest_SignInput()
        {
            Assert.IsTrue(_transaction.Input.Verify(_transaction.Hash()));
        }

        [TestMethod]
        [Description("validates a valid transaction")]
        public void TransactionTest_ValidateValidTransaction()
        {
            Assert.IsTrue(_transaction.IsValid());
        }

        [TestMethod]
        [Description("invalidates transaction with invalid output amount")]
        public void TransactionTest_InvalidateTransactionWithInvalidOutputAmount()
        {
            _transaction.Outputs[_senderWallet.Address].Amount = 999999;
            Assert.IsFalse(_transaction.IsValid());
        }

        [TestMethod]
        [Description("invalidates transaction with incorrect input signature")]
        public void TransactionTest_InvalidateTransactionWithIncorrectInputSignature()
        {
            _transaction.Input.Signature = new CustomWallet().Create().Sign("invalid-data".ToUTF8ByteArray());
            Assert.IsFalse(_transaction.IsValid());
        }

        [TestMethod]
        [Description("transaction update with invalid 'amount' throws an error")]
        public void TransactionTest_UpdateWithInvalidAmountThrowsError()
        {
            const int amount = 999999;
            Assertutils.ThrowsExceptionWithMessage(
                () => _transaction.Update(_senderWallet, "test-recipient", amount),
                $"Amount ({amount}) exceeds spendable balance: {_transaction.Outputs[_senderWallet.Address].Amount}");
        }


        private class ForUpdateTransactionTests
        {
            public string OriginalSignature { get; set; }
            public decimal OriginalSenderOutputAmount { get; set; }
            public string NextRecipient { get; set; }
            public decimal NextAmount { get; set; }
        }

        private ForUpdateTransactionTests InitForUpdateTransactionTests()
        {
            var futt = new ForUpdateTransactionTests
            {
                OriginalSignature = _transaction.Input.Signature,
                OriginalSenderOutputAmount = _transaction.Outputs[_senderWallet.Address].Amount,
                NextRecipient = "next-recipient",
                NextAmount = 50
            };
            
            _transaction.UpdateAsValid(_senderWallet, futt.NextRecipient, futt.NextAmount);
            return futt;
        }

        [TestMethod]
        [Description("outputs the 'amount' to the next recipient")]
        public void TransactionTest_OutputAmountToNextRecipient()
        {
            var futt = InitForUpdateTransactionTests();
            Assert.AreEqual(_transaction.Outputs[futt.NextRecipient].Amount, futt.NextAmount);
        }

        [TestMethod]
        [Description("subtracts the amount from the original sender output amount")]
        public void TransactionTest_SubtractNextAmountFromSendersOutput()
        {
            var futt = InitForUpdateTransactionTests();
            Assert.AreEqual(_transaction.Outputs[_senderWallet.Address].Amount, futt.OriginalSenderOutputAmount - futt.NextAmount);
        }

        [TestMethod]
        [Description("maintains a total output that matches the input amount")]
        public void TransactionTest_MaintainTotalOutputMatchingInputAmount()
        {
            Assert.AreEqual(_transaction.Outputs.Values.Select(o => o.Amount).Sum(), _transaction.Input.Amount);
        }

        [TestMethod]
        [Description("re-signs transaction")]
        public void TransactionTest_ReSignTransaction()
        {
            var futt = InitForUpdateTransactionTests();
            Assert.AreNotEqual(_transaction.Input.Signature, futt.OriginalSignature);
        }


        private class ForAnotherUpdateOfTheSameRecipientTransactionTests
        {
            public decimal OriginalSenderOutputAmount { get; set; }
            public string NextRecipient { get; set; }
            public decimal NextAmount { get; set; }
            public decimal AddedAmount { get; set; }
        }

        private ForAnotherUpdateOfTheSameRecipientTransactionTests InitForAnotherUpdateOfTheSameRecipientTransactionTests()
        {
            var futt = InitForUpdateTransactionTests();
            var fauotsr = new ForAnotherUpdateOfTheSameRecipientTransactionTests
            {
                OriginalSenderOutputAmount = futt.OriginalSenderOutputAmount,
                NextRecipient = futt.NextRecipient,
                NextAmount = futt.NextAmount,
                AddedAmount = 80
            };
            _transaction.UpdateAsValid(_senderWallet, fauotsr.NextRecipient, fauotsr.AddedAmount);
            return fauotsr;
        }

        [TestMethod]
        [Description("adds to the recipient amount")]
        public void TransactionTest_AddToRecipientAmount()
        {
            var fauotsr = InitForAnotherUpdateOfTheSameRecipientTransactionTests();
            Assert.AreEqual(_transaction.Outputs[fauotsr.NextRecipient].Amount, fauotsr.NextAmount + fauotsr.AddedAmount);
        }

        [TestMethod]
        [Description("subtracts the amount from the original sender output amount")]
        public void TransactionTest_SubtractFromOriginalSenderOutput()
        {
            var fauotsr = InitForAnotherUpdateOfTheSameRecipientTransactionTests();
            Assert.AreEqual(_transaction.Outputs[_senderWallet.Address].Amount, fauotsr.OriginalSenderOutputAmount - fauotsr.NextAmount - fauotsr.AddedAmount);
        }


        public class ForRewardTransactionTests
        {
            public Transaction RewardTransaction { get; set; }
            public CustomWallet MinerWallet { get; set; }
        }

        public ForRewardTransactionTests InitForRewardTransactionTests()
        {
            var frtt = new ForRewardTransactionTests { MinerWallet = new CustomWallet().Create() };
            frtt.RewardTransaction = Transaction.Reward(frtt.MinerWallet);
            return frtt;
        }

        [TestMethod]
        [Description("creates a transaction with the reward input")]
        public void TransactionTest_CreateTransactionWithRewardInput()
        {
            var frtt = InitForRewardTransactionTests();
            var blockchainWallet = CustomWallet.BlockchainWallet();
            var rewardTransaction = new Transaction().Create(blockchainWallet, frtt.MinerWallet.Address, MINING_REWARD, null,
                new Dictionary<string, TransactionOutput>
                {
                    [frtt.MinerWallet.Address] = new TransactionOutput { Amount = MINING_REWARD, Address = frtt.MinerWallet.Address }
                },
                new TransactionInput
                {
                    TimeStamp = UnixTimestamp.UtcNow(),
                    Amount = MINING_REWARD,
                    PublicKey = blockchainWallet.PublicKey,
                    Address = REWARD_INPUT_ADDRESS
                }, true);
            var rewardInput = rewardTransaction.Input;

            // signatures will be different even for the same data so I am verifying them first
            Assert.IsTrue(frtt.RewardTransaction.Input.Verify(frtt.RewardTransaction.Hash()));
            Assert.IsTrue(rewardTransaction.Input.Verify(rewardTransaction.Hash()));

            // and then I am removing them from inputs
            const string sameSignature = "same-signature";
            frtt.RewardTransaction.Input.Signature = sameSignature;
            rewardTransaction.Input.Signature = sameSignature;

            // I am also making sure that timestamps are the same
            var ts = UnixTimestamp.UtcNow();
            frtt.RewardTransaction.Input.TimeStamp = ts;
            rewardTransaction.Input.TimeStamp = ts;

            Assert.AreEqual(frtt.RewardTransaction.Input, rewardInput);
        }

        [TestMethod]
        [Description("creates one transaction for the miner with the `MINING_REWARD`")]
        public void TransactionTest_CreateOneTransactionForMinerWithMiningReward()
        {
            var frtt = InitForRewardTransactionTests();
            Assert.AreEqual(frtt.RewardTransaction.Outputs[frtt.MinerWallet.Address].Amount, MINING_REWARD);
        }
    }
}
