using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using Microsoft.Owin.Hosting;

namespace BlockchainApp.Source.Servers
{
    public class HttpServer : InformationSender
    {
        public string Address { get; }
        private readonly int _httpPort;

        public HttpServer(int httpPort)
        {
            _httpPort = httpPort;
            Address = $"{"localhost".ToIP()}:{httpPort}";
        }

        public void Listen()
        {
            WebApp.Start<Startup>($"http://{Address}/");
            OnInformationSending($@"Listening on port {_httpPort}");
        }
    }
}
