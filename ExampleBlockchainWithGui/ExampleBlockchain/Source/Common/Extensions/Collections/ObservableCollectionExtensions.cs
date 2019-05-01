using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BlockchainApp.Source.Common.Extensions.Collections
{
    public static class ObservableCollectionExtensions
    {
        public static ObservableCollection<T> ReplaceAll<T>(this ObservableCollection<T> obsCol, IEnumerable<T> newEnumerable)
        {
            var list = newEnumerable.ToList();
            obsCol.RemoveAll();
            obsCol.AddRange(list);
            return obsCol;
        }

        public static ObservableCollection<T> ReplaceAll<T>(this ObservableCollection<T> obsCol, T newEl)
        {
            return obsCol.ReplaceAll(newEl.ToEnumerable());
        }
    }
}
