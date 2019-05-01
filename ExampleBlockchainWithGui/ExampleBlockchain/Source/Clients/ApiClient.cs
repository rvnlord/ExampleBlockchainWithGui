using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoBot.Source.Clients;
using RestSharp;
using Truncon.Collections;
using WebSocketSharp;
using static BlockchainApp.Source.Common.Utils.LockUtils;

namespace BlockchainApp.Source.Clients
{
    public abstract class ApiClient
    {
        protected static readonly object _syncRateLimit = new object();
        protected string _baseAddress;
        protected string _address;
        protected TimeSpan _rateLimit;
        protected WebSocket _socket;

        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }

        public DateTime LastApiCallTimestamp { get; set; }

        protected async Task RateLimitAsync()
        {
            await Task.Run(() =>
            {
                Lock(_syncRateLimit, nameof(_syncRateLimit), nameof(RateLimitAsync), () =>
                {
                    var elapsedSpan = DateTime.Now - LastApiCallTimestamp;
                    if (elapsedSpan < _rateLimit)
                        Thread.Sleep(_rateLimit - elapsedSpan);
                    LastApiCallTimestamp = DateTime.Now;
                });
            });
        }

        protected abstract Task<T> QueryAsync<T>(QueryType queryType, Method method, string action, OrderedDictionary<string, string> parameters = null, DeserializeCustom<T> deserializer = null) where T : ResponseBase;
        
        private async Task<T> QueryPrivateAsync<T>(Method method, string action, OrderedDictionary<string, string> parameters = null, DeserializeCustom<T> deserializer = null) where T : ResponseBase
        {
            return await QueryAsync(QueryType.Private, method, action, parameters, deserializer);
        }

        private async Task<T> QueryPublicAsync<T>(Method method, string action, OrderedDictionary<string, string> parameters = null, DeserializeCustom<T> deserializer = null) where T : ResponseBase
        {
            return await QueryAsync(QueryType.Public, method, action, parameters, deserializer);
        }

        protected async Task<T> GetPrivateAsync<T>(string action, OrderedDictionary<string, string> parameters = null, DeserializeCustom<T> deserializer = null) where T : ResponseBase
        {
            return await QueryPrivateAsync(Method.GET, action, parameters, deserializer);
        }

        protected async Task<T> PostPrivateAsync<T>(string action, OrderedDictionary<string, string> parameters = null, DeserializeCustom<T> deserializer = null) where T : ResponseBase
        {
            return await QueryPrivateAsync(Method.POST, action, parameters, deserializer);
        }

        protected async Task<T> GetPublicAsync<T>(string action, OrderedDictionary<string, string> parameters = null, DeserializeCustom<T> deserializer = null) where T : ResponseBase
        {
            return await QueryPublicAsync(Method.GET, action, parameters, deserializer);
        }

        protected async Task<T> PostPublicAsync<T>(string action, OrderedDictionary<string, string> parameters = null, DeserializeCustom<T> deserializer = null) where T : ResponseBase
        {
            return await QueryPublicAsync(Method.POST, action, parameters, deserializer);
        }

        public delegate T DeserializeCustom<out T>(string content) where T : ResponseBase;
    }

    public enum QueryType
    {
        Private,
        Public
    }
}
