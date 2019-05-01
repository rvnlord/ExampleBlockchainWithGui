using System.Reflection;
using WebSocketSharp;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class WebSocketSharpWebSocketExtensions
    {
        public static string Id(this WebSocket socket)
        {
            return socket.GetType().GetField("_base64Key", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(socket).ToString();
        }
    }
}
