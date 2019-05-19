using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BlockchainApp.Source.Clients.PeersClient.Responses;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Constrollers;
using BlockchainApp.Source.Models.Wallets;
using RestSharp;
using Truncon.Collections;

namespace BlockchainApp.Source.Clients.PeersClient
{
    public class PeersClient : ApiClient
    {
        public PeersClient(string address, TimeSpan? rateLimit = null)
        {
            _address = address;
            _rateLimit = rateLimit ?? TimeSpan.FromSeconds(0);
        }

        public async Task<BlockResponse> BlockAsync(int id)
        {
            var parameters = new OrderedDictionary<string, string>
            {
                ["id"] = id.ToString()
            };

            return await GetPublicAsync(nameof(BlockchainApiController.Block), parameters, json => new BlockResponse().Parse(json));
        }

        public async Task<BlocksResponse> BlocksAsync()
        {
            return await GetPublicAsync(nameof(BlockchainApiController.Blocks), null, json => new BlocksResponse().Parse(json));
        }

        public async Task<BlockchainLengthResponse> BlocksLengthAsync()
        {
            return await GetPublicAsync(nameof(BlockchainApiController.BlockchainLength), null, json => new BlockchainLengthResponse().Parse(json));
        }

        public async Task<KnownAddressesResponse> KnownAddressesAsync()
        {
            return await GetPublicAsync(nameof(BlockchainApiController.KnownAddresses), null, json => new KnownAddressesResponse().Parse(json));
        }

        public async Task<MineArtificialResponse> MineArtificialAsync(Dictionary<string, Transaction> transactions)
        {
            var parameters = new OrderedDictionary<string, string>
            {
                ["Transactions"] = transactions.JsonSerialize()
            };

            return await PostPublicAsync(nameof(BlockchainApiController.MineArtificial), parameters, json => new MineArtificialResponse().Parse(json));
        }

        public async Task<MineTransactionsResponse> MineTransactionsAsync()
        {
            return await GetPublicAsync(nameof(BlockchainApiController.MineTransactions), null, json => new MineTransactionsResponse().Parse(json));
        }

        public async Task<SendResponse> SendAsync(string recipient, decimal amount)
        {
            var parameters = new OrderedDictionary<string, string>
            {
                ["Recipient"] = recipient,
                ["Amount"] = $"{amount:0.########}"
            };

            return await PostPublicAsync(nameof(BlockchainApiController.Send), parameters, json => new SendResponse().Parse(json));
        }

        public async Task<TransactionsResponse> TransactionsAsync()
        {
            return await GetPublicAsync(nameof(BlockchainApiController.Transactions), null, json => new TransactionsResponse().Parse(json));
        }

        public async Task<WalletInfoResponse> WalletInfoAsync()
        {
            return await GetPublicAsync(nameof(BlockchainApiController.WalletInfo), null, json => new WalletInfoResponse().Parse(json));
        }

        protected override async Task<T> QueryAsync<T>(QueryType queryType, Method method, string action, OrderedDictionary<string, string> parameters = null, DeserializeCustom<T> deserializer = null)
        {
            await RateLimitAsync();

            var uri = $"http://{_address}/{action}/";
            var request = new RestRequest(method); // only public requests for now

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("User-Agent", "cs-p2p-blockchain");

            if (parameters?.Any() == true)
            {
                const string idn = "id";
                var idParam = parameters.KVorN(idn);
                var hasId = idParam.Value != null;
                var bodyParams = (hasId ? parameters.Where(p => p.Key != idn) : parameters).ToOrderedDictionary(p => p.Key, p => p.Value);

                if (hasId)
                    uri += idParam.Value;
                if (bodyParams.Any())
                    request.AddParameter("application/json", bodyParams.JsonSerialize(), ParameterType.RequestBody); // all params except id are SINGLE jsonObject parameter appended to body
            }

            var rawResponse = await new RestClient(uri).ExecuteTaskAsync(request);
            if (rawResponse.StatusCode != HttpStatusCode.OK)
                throw new PeersClientException(rawResponse.StatusCode.EnumToString().AddSpacesToPascalCase());
            if (rawResponse.ContentLength <= 0)
                throw new PeersClientException("Peer returned an empty content");
            var response = deserializer == null
                ? rawResponse.Content.JsonDeserialize().To<T>()
                : deserializer(rawResponse.Content);
            return response;
        }
    }
}
