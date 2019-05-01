using System.Collections.Generic;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Models.Wallets;

namespace BlockchainApp.Source.Clients.PeersClient.Responses
{
    public class TransactionsResponse : PeersClientResponse
    {
        public Dictionary<string, Transaction> Transactions { get; set; }

        public TransactionsResponse Parse(string json)
        {
            HandleErrors(json);

            Transactions = json.JsonDeserialize().To<Dictionary<string, Transaction>>();

            return this;
        }
    }
}
