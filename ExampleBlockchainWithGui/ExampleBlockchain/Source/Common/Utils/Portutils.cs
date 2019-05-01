using System.Linq;
using System.Net.NetworkInformation;
using BlockchainApp.Source.Common.Extensions.Collections;

namespace BlockchainApp.Source.Common.Utils
{
    public static class Portutils
    {
        public static int[] PortsInUse()
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            var tcpListenersArray = ipGlobalProperties.GetActiveTcpListeners();
            var udpListenersArray = ipGlobalProperties.GetActiveUdpListeners();
            var portsInUse = tcpConnInfoArray.Select(i => i.LocalEndPoint.Port).ConcatMany(
                    tcpListenersArray.Select(i => i.Port), udpListenersArray.Select(i => i.Port))
                .Distinct().OrderBy(p => p).ToArray();
            return portsInUse;
        }
    }
}
