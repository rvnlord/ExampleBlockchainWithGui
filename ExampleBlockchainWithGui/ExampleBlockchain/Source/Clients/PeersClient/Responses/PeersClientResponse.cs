using System.Collections.Generic;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using CryptoBot.Source.Clients;

namespace BlockchainApp.Source.Clients.PeersClient.Responses
{
    public class PeersClientResponse : ResponseBase
    {
        public string Error { get; set; }

        public PeersClientResponse RawParse(string json)
        {
            var rawGenericReposnse = json.JsonDeserialize().To<Dictionary<string, object>>();
            Error = rawGenericReposnse?.VorN("Type")?.ToString().EqIgnoreCase("Error") == true 
                ? rawGenericReposnse["Message"].ToString() 
                : null;

            return this;
        }

        public void HandleErrors(string json)
        {
            RawParse(json);
            if (Error != null)
                throw new PeersClientException(Error);
        }
    }
}
