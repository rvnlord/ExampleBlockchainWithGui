using System.Collections.Generic;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Models;

namespace BlockchainApp.Source.Clients.PeersClient.Responses
{
    public class MineTransactionsResponse : PeersClientResponse
    {
        public List<Block> Chain { get; set; }

        public MineTransactionsResponse Parse(string json)
        {
            HandleErrors(json);

            Chain = json.JsonDeserialize()["Message"].To<List<Block>>();

            return this;
        }
    }
}
