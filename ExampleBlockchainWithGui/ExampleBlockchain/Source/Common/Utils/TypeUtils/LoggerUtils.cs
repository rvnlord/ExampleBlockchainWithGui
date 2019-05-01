using System.Reflection;
using NLog;

namespace BlockchainApp.Source.Common.Utils.TypeUtils
{
    public class LoggerUtils
    {
        public static Logger Create()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = "ErrorLog.log",
                Layout = "${longdate}: ${level:uppercase=true} | ${logger} | ${message}"
            };

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;

            return LogManager.GetLogger("Main");
        }

        public static NLog.Logger Get(MethodBase method) => LogManager.GetLogger(method.DeclaringType?.FullName);

        public static void Close()
        {
            NLog.LogManager.Shutdown();
        }
    }
}
