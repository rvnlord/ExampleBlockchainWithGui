using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils;
using BlockchainApp.Source.Common.Utils.TypeUtils;
using Infragistics.Controls.Interactions;
using MoreLinq;

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
