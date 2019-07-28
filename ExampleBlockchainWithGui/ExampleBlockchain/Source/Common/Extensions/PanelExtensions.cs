using System.Windows.Controls;
using BlockchainApp.Source.Common.Utils;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class PanelExtensions
    {
        public static void ShowLoader(this Panel control)
        {
            AsyncUtils.ShowLoader(control);
        }

        public static void HideLoader(this Panel control)
        {
            AsyncUtils.HideLoader(control);
        }

        public static bool HasLoader(this Panel control)
        {
            return AsyncUtils.HasLoader(control);
        }
    }
}
