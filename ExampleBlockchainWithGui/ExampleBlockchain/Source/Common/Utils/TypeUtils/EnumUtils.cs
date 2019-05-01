using System;
using System.Collections.Generic;
using System.Linq;
using BlockchainApp.Source.Common.Utils.UtilClasses;

namespace BlockchainApp.Source.Common.Utils.TypeUtils
{
    public static class EnumUtils
    {
        public static List<DdlItem> EnumToDdlItems<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<int>().Select(i => new DdlItem(i, Enum.GetName(typeof(T), i))).ToList();
        }

        public static List<DdlItem> EnumToDdlItems<T>(Func<T, string> customNamesConverter)
        {
            return EnumToDdlItems<T>()
                .Select(item =>
                    new DdlItem(
                        item.Index,
                        customNamesConverter((T)Enum.ToObject(typeof(T), item.Index))))
                .ToList();
        }

        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
