using BlockchainApp.Source.Common.Extensions;

namespace BlockchainApp.Source.Clients.PeersClient.Responses
{
    public class BlockchainLengthResponse : PeersClientResponse
    {
        public int BlockchainLength { get; set; }

        public BlockchainLengthResponse Parse(string json)
        {
            HandleErrors(json);

            BlockchainLength = json.JsonDeserialize()["BlockchainLength"].To<int>();

            return this;
        }
    }
}
