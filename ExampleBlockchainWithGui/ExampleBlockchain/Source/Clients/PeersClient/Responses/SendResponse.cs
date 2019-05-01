using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Models.Wallets;

namespace BlockchainApp.Source.Clients.PeersClient.Responses
{
    public class SendResponse : PeersClientResponse
    {
        public Transaction Transaction { get; set; }

        public SendResponse Parse(string json)
        {
            HandleErrors(json);

            Transaction = json.JsonDeserialize()["Message"].To<Transaction>();

            return this;
        }
    }
}
