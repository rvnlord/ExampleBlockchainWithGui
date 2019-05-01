using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BlockchainApp.Source.Common.Extensions.Collections
{
    public static class ItemCollectionExtensions
    {
        public static T[] ToArray<T>(this ItemCollection list)
        {
            var array = new T[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        public static object[] ToArray(this ItemCollection list)
        {
            var array = new object[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        public static ItemCollection Swap(this ItemCollection items, int i, int j)
        {
            var tabItem = items[i];
            items.Remove(tabItem);
            items.Insert(j, tabItem);
            return items;
        }

        public static ItemCollection OrderWith(this ItemCollection items, IEnumerable<int> orderPattern)
        {
            var orderedItems = items.ToArray().OrderWith(orderPattern);
            items.Clear();
            items.AddRange(orderedItems);
            return items;
        }

        public static void AddRange(this ItemCollection list, IEnumerable items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            foreach (var current in items)
                list.Add(current);
        }

        public static void RemoveRange(this ItemCollection items, IEnumerable<object> enuemrable)
        {
            foreach (var o in enuemrable)
                items.Remove(o);
        }

        public static object Last(this ItemCollection col)
        {
            return col[col.Count - 1];
        }
    }
}
