using System.Text;
using System.Windows.Media;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using MahApps.Metro.IconPacks;

namespace BlockchainApp.Source.Models.ViewModels
{
    public class TransactionGvVM : BaseVM
    {
        private string _id;
        private UnixTimestamp _timeStamp;
        private TransactionGvVMType _transactionType;
        private TransactionGvVMState _transactionState;
        private string _from;
        private decimal _amount;
        private string _signature;
        private string _to;

        public string Id { get => _id; set => SetPropertyAndNotify(ref _id, value, nameof(Id)); }
        public UnixTimestamp TimeStamp { get => _timeStamp; set => SetPropertyAndNotify(ref _timeStamp, value, nameof(TimeStamp)); }
        public TransactionGvVMType TransactionType { get => _transactionType; set => SetPropertyAndNotify(ref _transactionType, value, nameof(TransactionType)); }
        public TransactionGvVMState TransactionState { get => _transactionState; set => SetPropertyAndNotify(ref _transactionState, value, nameof(TransactionState)); }
        public string From { get => _from; set => SetPropertyAndNotify(ref _from, value, nameof(From)); }
        public decimal Amount { get => _amount; set => SetPropertyAndNotify(ref _amount, value, nameof(Amount)); }
        public string Signature { get => _signature; set => SetPropertyAndNotify(ref _signature, value, nameof(Signature)); }
        public string To { get => _to; set => SetPropertyAndNotify(ref _to, value, nameof(To)); }
        public string WalletAddress { get; set; }

        public string TimeString => TimeStamp.ToExtendedTime().ToLocal().ToDateTimeString();
        public string TransactionTypeString => TransactionType.EnumToString();
        public PackIconMaterial TransactionTypeIcon => new PackIconMaterial
        {
            Kind = _transactionType == TransactionGvVMType.Incoming
                ? PackIconMaterialKind.ArrowCollapseLeft
                : _transactionType == TransactionGvVMType.Outgoing
                    ? PackIconMaterialKind.Share
                    : _transactionType == TransactionGvVMType.Reward
                        ? PackIconMaterialKind.ArrowDownBoldHexagonOutline
                        : PackIconMaterialKind.Help,
            Foreground = _transactionType.In(TransactionGvVMType.Incoming, TransactionGvVMType.Reward)
                ? Brushes.YellowGreen : new SolidColorBrush(Color.FromRgb(253, 86, 87))
        };
        public string AmountString
        {
            get
            {
                var sbAmount = new StringBuilder();
                if (_transactionType.In(TransactionGvVMType.Incoming, TransactionGvVMType.Reward))
                    sbAmount.Append("+");
                else if (_transactionType == TransactionGvVMType.Outgoing)
                    sbAmount.Append("-");
                sbAmount.Append($"{Amount:#.########} EXC");
                return sbAmount.ToString();
            }
        }
        public string SenderOrRecipientString => _transactionType == TransactionGvVMType.Incoming 
            ? _from 
            : _transactionType == TransactionGvVMType.Outgoing 
                ? _to 
                : "-";
        public string TransactionStateString => _transactionState.EnumToString();
        public PackIconMaterial TransactionStateIcon => new PackIconMaterial
        {
            Kind = _transactionState == TransactionGvVMState.Confirmed
                ? PackIconMaterialKind.CheckboxMarkedCircleOutline
                : PackIconMaterialKind.ProgressClock,
            Foreground = _transactionState == TransactionGvVMState.Confirmed
                ? Brushes.YellowGreen : new SolidColorBrush(Color.FromRgb(253, 86, 87))
        };


        public override string ToString()
        {
            return
                "Transaction - \n" +
                $"    Time               : {TimeString}\n" +
                $"    Type               : {TransactionTypeString}\n" +
                $"    Amount             : {AmountString}\n" +
                $"    Sender/Recipient   : {SenderOrRecipientString}\n" +
                $"    State              : {TransactionStateString}\n" +
                $"    Id                 : {Id}\n" +
                $"    Signature          : {Signature}";
        }
    }

    public enum TransactionGvVMType
    {
        Incoming,
        Outgoing,
        Reward,
        Unspecified
    }

    public enum TransactionGvVMState
    {
        Confirmed,
        Unconfirmed
    }
}
