using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Models;

namespace BlockchainApp.Source.Clients.PeersClient.Responses
{
    public class BlockResponse : PeersClientResponse
    {
        public int BlockNo { get; set; }
        public Block Block { get; set; }
        
        public BlockResponse Parse(string json)
        {
            HandleErrors(json);

            var jToken = json.JsonDeserialize();
            Block = jToken["Block"].To<Block>();
            BlockNo = jToken["BlockNo"].To<int>();

            return this;
        }
    }
}
