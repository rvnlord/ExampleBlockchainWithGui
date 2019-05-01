namespace BlockchainApp.Source.Models.Wallets
{
    public class TransactionOutput
    {
        public decimal Amount { get; set; }
        public string Address { get; set; }

        public TransactionOutput() { }

        public TransactionOutput(TransactionOutput output)
        {
            Amount = output.Amount;
            Address = output.Address;
        }

        public override bool Equals(object o)
        {
            if (!(o is TransactionOutput)) return false;
            var that = (TransactionOutput)o;
            return Amount == that.Amount &&
                Address == that.Address;
        }

        public override int GetHashCode()
        {
            return Amount.GetHashCode() ^ 3 *
                Address.GetHashCode() ^ 5;
        }
    }
}
