using System.Windows.Controls;
using System.Windows.Data;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class DataGridTextColumnExtensions
    {
        public static string DataMemberName(this DataGridTextColumn column)
        {
            return ((Binding)column.Binding).Path.Path;
        }
    }
}
