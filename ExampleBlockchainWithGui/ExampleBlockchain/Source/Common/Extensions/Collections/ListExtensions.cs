using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlockchainApp.Source.Common.Converters;
using BlockchainApp.Source.Models;
using BlockchainApp.Source.Models.ViewModels;
using BlockchainApp.Source.Models.Wallets;

namespace BlockchainApp.Source.Common.Extensions.Collections
{
    public static class ListExtensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static void RemoveBy<TSource>(this List<TSource> source, Func<TSource, bool> selector) where TSource : class
        {
            var src = source.ToArray();
            foreach (var entity in src)
            {
                if (selector(entity))
                    source.Remove(entity);
            }
        }

        public static void RemoveByMany<TSource, TKey>(this List<TSource> source, Func<TSource, TKey> selector, IEnumerable<TKey> matches) where TSource : class
        {
            foreach (var match in matches)
                source.RemoveBy(e => Equals(selector(e), match));
        }

        public static T[] IListToArray<T>(this IList<T> list)
        {
            var array = new T[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        public static object[] IListToArray(this IList list)
        {
            var array = new object[list.Count];
            for (var i = 0; i < list.Count; i++)
                array[i] = list[i];
            return array;
        }

        public static List<T> ReplaceAll<T>(this List<T> list, IEnumerable<T> en)
        {
            var newList = en.ToList();
            list.RemoveAll();
            list.AddRange(newList);
            return list;
        }

        public static List<T> ReplaceAll<T>(this List<T> list, T newEl)
        {
            return list.ReplaceAll(newEl.ToEnumerable());
        }

        public static void RemoveAll<T>(this IList<T> collection)
        {
            while (collection.Count != 0)
                collection.RemoveAt(0);
        }

        public static int RemoveAll(this IList list, Predicate<object> match)
        {
            var list2 = list.Cast<object>().Where(current => match(current)).ToList();
            foreach (var current2 in list2)
                list.Remove(current2);
            return list2.Count;
        }

        public static void AddRange(this IList list, IEnumerable items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            foreach (var current in items)
                list.Add(current);
        }

        public static void RemoveRange(this IList list, IEnumerable items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            foreach (var current in items)
                list.Remove(current);
        }

        public static void RemoveIfExists<T>(this List<T> list, T item)
        {
            if (list.Contains(item))
                list.Remove(item);
        }

        public static T Extract<T>(this List<T> list, T n)
        {
            var index = list.FindIndex(x => Equals(x, n));
            var result = list[index];
            list.RemoveAt(index);
            return result;
        }

        public static T Push<T>(this List<T> list, T n)
        {
            list.Add(n);
            return n;
        }

        public static List<T> Replace<T>(this List<T> list, T oldEl, T newEl)
        {
            var idx = list.Index(oldEl);
            list[idx] = newEl;
            return list;
        }
    }
}
