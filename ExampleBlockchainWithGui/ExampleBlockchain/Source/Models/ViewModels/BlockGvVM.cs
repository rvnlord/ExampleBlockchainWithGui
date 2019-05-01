using BlockchainApp.Source.Common.Utils.UtilClasses;

namespace BlockchainApp.Source.Models.ViewModels
{
    public class BlockGvVM : BaseVM
    {
        private UnixTimestamp _timeStamp;
        private string _hash;
        private int _difficulty;
        private int _transactionsCount;

        public UnixTimestamp TimeStamp { get => _timeStamp; set => SetPropertyAndNotify(ref _timeStamp, value, nameof(TimeStamp)); }
        public string Hash { get => _hash; set => SetPropertyAndNotify(ref _hash, value, nameof(Hash)); }
        public int Difficulty { get => _difficulty; set => SetPropertyAndNotify(ref _difficulty, value, nameof(Difficulty)); }
        public int TransactionsCount { get => _transactionsCount; set => SetPropertyAndNotify(ref _transactionsCount, value, nameof(TransactionsCount)); }

        public string TimeString => TimeStamp.ToExtendedTime().ToLocal().ToDateTimeString();

        public override string ToString()
        {
            return 
                "Block - \n" +
                $"    Timestamp          : {TimeString}\n" +
                $"    Hash               : {Hash}\n" +
                $"    Difficulty         : {Difficulty}\n" +
                $"    Transactions Count : {TransactionsCount}";
        }
    }
}
