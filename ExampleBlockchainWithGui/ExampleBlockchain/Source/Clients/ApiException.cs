using System;
using System.Runtime.Serialization;
using BlockchainApp.Source.Common.Extensions;

namespace BlockchainApp.Source.Clients
{
    [Serializable]
    public class ApiException : Exception
    {
        public ApiException() { }
        public ApiException(string message) : base(message) { }
        public ApiException(string message, Exception innerException) : base(message, innerException) { }
        protected ApiException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        protected static string CreateMessage(string exClassName, string originalMessage)
        {
            return $"({exClassName.BeforeFirst("Exception")}) {originalMessage}";
        }
    }
}
