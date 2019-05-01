using System.Collections.Generic;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Models;

namespace BlockchainApp.Source.Clients.PeersClient.Responses
{
    public class BlocksResponse : PeersClientResponse
    {
        public List<Block> Blocks { get; set; }

        public BlocksResponse Parse(string json)
        {
            HandleErrors(json);

            Blocks = json.JsonDeserialize().To<List<Block>>();

            return this;
        }
    }
}
