using System.Collections.Generic;
using BlockchainApp.Source.Common.Extensions;

namespace BlockchainApp.Source.Clients.PeersClient.Responses
{
    public class KnownAddressesResponse : PeersClientResponse
    {
        public List<string> KnownAddresses { get; set; }

        public KnownAddressesResponse Parse(string json)
        {
            HandleErrors(json);

            KnownAddresses = json.JsonDeserialize()["KnownAddresses"].To<List<string>>();

            return this;
        }
    }
}
