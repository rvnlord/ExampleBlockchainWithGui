using System;
using System.Linq;
using System.Windows.Controls;
using BlockchainApp.Source.Common.Utils.UtilClasses;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class ComboBoxExtensions
    {
        public static void SelectByItem(this ComboBox ddl, DdlItem item)
        {
            ddl.SelectedItem = ddl.ItemsSource.Cast<DdlItem>().Single(i => i.Equals(item));
        }

        public static void SelectById(this ComboBox ddl, int customId)
        {
            var item = ddl.ItemsSource.Cast<DdlItem>().Single(i => i.Index == customId);
            ddl.SelectedItem = item;
        }

        public static void SelectByText(this ComboBox ddl, string customText)
        {
            var item = ddl.ItemsSource.Cast<DdlItem>().Single(i => i.Text == customText);
            ddl.SelectedItem = item;
        }

        public static DdlItem SelectedItem(this ComboBox ddl)
        {
            return (DdlItem)ddl.SelectedItem;
        }

        public static int SelectedId(this ComboBox ddl)
        {
            return ((DdlItem)ddl.SelectedItem).Index;
        }

        public static string SelectedText(this ComboBox ddl)
        {
            return ((DdlItem)ddl.SelectedItem).Text;
        }

        public static T SelectedEnumValue<T>(this ComboBox ddl)
        {
            var selectedItem = (DdlItem)ddl.SelectedItem;
            var enumType = typeof(T);

            var value = (Enum)Enum.ToObject(enumType, selectedItem.Index);
            if (Enum.IsDefined(enumType, value) == false)
                throw new NotSupportedException($"Nie można przekonwertować wartości na podany typ: {enumType}");

            return (T)(object)value;
        }
    }
}
