using System;
using System.Reflection;
using WebSocketSharp;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class WebSocketSharpLoggerExtensions
    {
        public static void Disable(this Logger logger)
        {
            var field = logger.GetType().GetField("_output", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(logger, new Action<LogData, string>((d, s) => { }));
        }
    }
}
