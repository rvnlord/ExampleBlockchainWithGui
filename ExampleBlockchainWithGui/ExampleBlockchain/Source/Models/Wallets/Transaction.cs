using System;
using System.Collections.Generic;
using System.Linq;
using BlockchainApp.Source.Common.Converters;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using BlockchainApp.Source.Models.ViewModels;
using static BlockchainApp.Source.Common.Utils.CryptoUtils;
using static BlockchainApp.Source.Config;

namespace BlockchainApp.Source.Models.Wallets
{
    public class Transaction : InformationSender
    {
        public string Id { get; set; }
        public TransactionInput Input { get; set; }
        public Dictionary<string, TransactionOutput> Outputs { get; set; }

        public Transaction()
        {
            Id = null;
            Input = null;
            Outputs = new Dictionary<string, TransactionOutput>();
        }

        public Transaction(Transaction transaction)
        {
            Id = transaction.Id;
            Input = new TransactionInput(transaction.Input);
            Outputs = transaction.Outputs.ToDictionary(kvp => kvp.Key, kvp => new TransactionOutput(kvp.Value));
        }

        public Transaction Create(CustomWallet senderWallet, string recipient, decimal amount, Blockchain bc = null, Dictionary<string, TransactionOutput> outputs = null, TransactionInput input = null, bool skipValidation = false)
        {
            if (bc?.Chain != null && bc.Chain.Any())
                senderWallet.UpdateBalance(bc);

            if (!skipValidation)
            {
                if (amount <= 0)
                    throw new Exception("You can't send 0");
                if (amount > senderWallet.Balance)
                    throw new Exception($"Amount: {amount} exceeds balance: {senderWallet.Balance}");
                if (senderWallet.Address == recipient)
                    throw new Exception($"You can't send transaction to yourself dummy!");
                if (!CustomWallet.IsAddressCorrect(recipient))
                    throw new Exception("This is not a correct address!");
            }

            Id = UniqueId();
            Outputs = outputs ?? CreateOutputs(senderWallet, recipient, amount);
            Input = input ?? CreateInput(senderWallet);

            Input.Signature = senderWallet.Sign(Hash());

            return this;
        }

        public Transaction CreateAsValid(CustomWallet senderWallet, string recipient, decimal amount, Blockchain bc = null)
        {
            return Create(senderWallet, recipient, amount, bc, null, null, true);
        }

        private static Dictionary<string, TransactionOutput> CreateOutputs(Wallet senderWallet, string recipient, decimal amount)
        {
            return new Dictionary<string, TransactionOutput> {
                [senderWallet.Address] = new TransactionOutput { Amount = senderWallet.Balance - amount, Address = senderWallet.Address },
                [recipient] = new TransactionOutput { Amount = amount, Address = recipient }
            };
        }

        private static TransactionInput CreateInput(Wallet senderWallet)
        {
            var input = new TransactionInput
            {
                TimeStamp = UnixTimestamp.UtcNow(),
                Amount = senderWallet.Balance,
                PublicKey = senderWallet.PublicKey,
                Address = senderWallet.Address
            };

            return input;
        }

        public byte[] Hash()
        {
            var data = Input.TimeStamp.ToNoCommaAccurateString(15) + $"{Input.Amount:0.########}" + // formatting is to prevent the bug when decimal remember how many decimal places it was intialized with and return those places on converting back to string, this breaks hashing
                Input.PublicKey + Input.Address +
                Outputs.Values.OrderBy(o => o.Address).JsonSerialize();
            //OnInformationSending(data);
            return Sha256(data.ToUTF8ByteArray());
        }

        public static Transaction Reward(CustomWallet minerWallet)
        {
            var blockchainWallet = CustomWallet.BlockchainWallet();
            var rewardTansaction = new Transaction().Create(blockchainWallet, minerWallet.Address, MINING_REWARD, null,
                new Dictionary<string, TransactionOutput>
                {
                    [minerWallet.Address] = new TransactionOutput { Amount = MINING_REWARD, Address = minerWallet.Address }
                },
                new TransactionInput
                {
                    TimeStamp = UnixTimestamp.UtcNow(),
                    Amount = MINING_REWARD,
                    PublicKey = blockchainWallet.PublicKey,
                    Address = REWARD_INPUT_ADDRESS
                }, true);
            return rewardTansaction;
        }

        public static Transaction Empty() => new Transaction();

        public Transaction Update(CustomWallet senderWallet, string recipient, decimal amount, bool skipValidation = false)
        {
            var senderOutput = Outputs[senderWallet.Address];

            if (!skipValidation)
            {
                if (amount <= 0)
                    throw new Exception("You can't send 0");
                if (amount > senderOutput.Amount)
                    throw new Exception($"Amount ({amount}) exceeds spendable balance: {senderOutput.Amount}");
                if (senderWallet.Address == recipient)
                    throw new Exception($"You can't send transaction to yourself dummy!");
                if (!CustomWallet.IsAddressCorrect(recipient))
                    throw new Exception("This is not a correct address!");
            }

            if (Outputs.VorN(recipient) == null)
                Outputs[recipient] = new TransactionOutput { Amount = amount, Address = recipient };
            else
                Outputs[recipient] = new TransactionOutput { Amount = Outputs[recipient].Amount + amount, Address = recipient };

            senderOutput.Amount -= amount;
            Input = CreateInput(senderWallet);
            Input.Signature = senderWallet.Sign(Hash());
            return this;
        }

        public Transaction UpdateAsValid(CustomWallet senderWallet, string recipient, decimal amount)
        {
            return Update(senderWallet, recipient, amount, true);
        }

        public bool IsValid()
        {
            var outputTotal = Outputs.Values.Select(o => o.Amount).Sum();

            if (Input?.Amount != outputTotal)
            {
                OnInformationSending($"Invalid transaction from {Input?.Address ?? "-"}");
                return false;
            }

            if(!Input.Verify(Hash()))
            {
                OnInformationSending($"Invalid signature from {Input.Address}");
                return false;
            }

            return true;
        }

        public List<TransactionGvVM> ToTransactionsGvVM() => TransactionConverter.ToTransactionsGvVM(this);

        public override bool Equals(object o)
        {
            if (!(o is Transaction)) return false;
            var that = (Transaction)o;
            return Id == that.Id &&
               Input == that.Input &&
               Outputs.CollectionEqual(that.Outputs);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ 3 *
               Input.GetHashCode() ^ 5 *
               Outputs.GetHashCode() ^ 7;
        }
    }
}
