using System.Collections.Generic;
using BlockchainApp.Source.Common.Extensions;

namespace BlockchainApp.Source.Clients.PeersClient.Responses
{
    public class WalletInfoResponse : PeersClientResponse
    {
        public string Address { get; set; }
        public string PublicKey { get; set; }
        public decimal Balance { get; set; }
        public decimal SpendableBalance { get; set; }

        public WalletInfoResponse Parse(string json)
        {
            HandleErrors(json);

            var walletInfo = json.JsonDeserialize().To<Dictionary<string, object>>(); // it can also bee deserialized directly to 'WalletInfoResponse' and mapped to this
            Address = walletInfo[nameof(Address)].ToString();
            PublicKey = walletInfo[nameof(PublicKey)].ToString();
            Balance = walletInfo[nameof(Balance)].ToDecimal();
            SpendableBalance = walletInfo[nameof(SpendableBalance)].ToDecimal();

            return this;
        }
    }
}
