using System.Threading.Tasks;
using BlockchainApp.Source.Common.Utils;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class TaskExtensions
    {
        public static void Sync(this Task task) => AsyncUtils.Sync(task);
        public static TResult Sync<TResult>(this Task<TResult> task) => AsyncUtils.Sync(task);
    }
}
