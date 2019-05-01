using System;
using System.Linq;
using System.Threading;
using static BlockchainApp.Source.Config;

namespace BlockchainApp.Source.Common.Utils
{
    public static class LockUtils
    {
        public static TResult Lock<TResult>(object syncObject, string syncObjectName, string methodName, Func<TResult> action)
        {
            var lockWasTaken = false;
            var temp = syncObject;
            try
            {
                _logger.Debug($"{methodName}() waiting for {syncObjectName}");
                Monitor.Enter(temp, ref lockWasTaken);
                _logger.Debug($"{methodName}() aquired {syncObjectName}");
                return action();
            }
            finally
            {
                if (lockWasTaken)
                {
                    Monitor.Exit(temp);
                    _logger.Debug($"{methodName}() released {syncObjectName}");
                }
                else
                    _logger.Debug($"{methodName}() lock was not taken {syncObjectName}");
            }
        }

        public static TResult Lock<TResult>(object[] syncObjects, string[] syncObjectNames, string methodName, Func<TResult> action)
        {
            var nLockWasTaken = syncObjects.Select(so => false).ToArray();
            var temps = syncObjects.ToArray();
            try
            {
                for (var i = 0; i < temps.Length; i++)
                {
                    _logger.Debug($"{methodName}() waiting for {syncObjectNames[i]}");
                    Monitor.Enter(temps[i], ref nLockWasTaken[i]);
                    _logger.Debug($"{methodName}() aquired {syncObjectNames[i]}");
                }
                
                return action();
            }
            finally
            {
                for (var i = temps.Length - 1; i >= 0; i--)
                {
                    if (nLockWasTaken[i])
                    {
                        Monitor.Exit(temps[i]);
                        _logger.Debug($"{methodName}() released {syncObjectNames[i]}");
                    }
                    else
                        _logger.Debug($"{methodName}() lock was not taken {syncObjectNames[i]}");
                }
            }
        }

        public static void Lock(object syncObject, string syncObjectName, string methodName, Action action)
        {
            var lockWasTaken = false;
            var temp = syncObject;
            try
            {
                _logger.Debug($"{methodName}() waiting for {syncObjectName}");
                Monitor.Enter(temp, ref lockWasTaken);
                _logger.Debug($"{methodName}() aquired {syncObjectName}");
                action();
            }
            finally
            {
                if (lockWasTaken)
                {
                    Monitor.Exit(temp);
                    _logger.Debug($"{methodName}() released {syncObjectName}");
                }
                else
                    _logger.Debug($"{methodName}() lock was not taken {syncObjectName}");
            }
        }

        public static void Lock(object[] syncObjects, string[] syncObjectNames, string methodName, Action action)
        {
            var nLockWasTaken = syncObjects.Select(so => false).ToArray();
            var temps = syncObjects.ToArray();
            try
            {
                for (var i = 0; i < temps.Length; i++)
                {
                    _logger.Debug($"{methodName}() waiting for {syncObjectNames[i]}");
                    Monitor.Enter(temps[i], ref nLockWasTaken[i]);
                    _logger.Debug($"{methodName}() aquired {syncObjectNames[i]}");
                }

                action();
            }
            finally
            {
                for (var i = temps.Length - 1; i >= 0; i--)
                {
                    if (nLockWasTaken[i])
                    {
                        Monitor.Exit(temps[i]);
                        _logger.Debug($"{methodName}() released {syncObjectNames[i]}");
                    }
                    else
                        _logger.Debug($"{methodName}() lock was not taken {syncObjectNames[i]}");
                }
            }
        }
    }
}
