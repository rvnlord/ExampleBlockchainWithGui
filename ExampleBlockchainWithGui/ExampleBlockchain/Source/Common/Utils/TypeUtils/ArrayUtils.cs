using System.Linq;

namespace BlockchainApp.Source.Common.Utils.TypeUtils
{
    public static class ArrayUtils
    {
        public static T[] ConcatMany<T>(params T[][] arrays) => arrays.SelectMany(x => x).ToArray();
    }
}
