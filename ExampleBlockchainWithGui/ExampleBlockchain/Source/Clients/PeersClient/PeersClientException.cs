using System;
using System.Runtime.Serialization;
using BlockchainApp.Source.Clients.PeersClient.Responses;

namespace BlockchainApp.Source.Clients.PeersClient
{
    [Serializable]
    public class PeersClientException : ApiException
    {
        public PeersClientResponse Response { get; }

        public PeersClientException(string message, PeersClientResponse response) : base(CreateMessage(nameof(PeersClientException), message))
        {
            Response = response;
        }

        public PeersClientException()
        {
        }

        public PeersClientException(string message) : base(CreateMessage(nameof(PeersClientException), message))
        {
        }

        public PeersClientException(string message, Exception innerException) : base(CreateMessage(nameof(PeersClientException), message), innerException)
        {
        }

        protected PeersClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue("Response", Response);
            base.GetObjectData(info, context);
        }
    }
}