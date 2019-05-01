using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsIEnumerableType(this Type type)
        {
            return type.GetInterface(nameof(IEnumerable)) != null;
        }

        public static bool IsIListType(this Type type)
        {
            return type.GetInterface(nameof(IList)) != null;
        }

        public static bool IsIDictionaryType(this Type type)
        {
            return type.GetInterface(nameof(IDictionary)) != null;
        }

        public static bool IsObservableCollectionType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObservableCollection<>);
        }
    }
}
