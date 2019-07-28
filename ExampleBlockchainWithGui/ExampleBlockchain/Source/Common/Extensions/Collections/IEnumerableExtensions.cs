using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BlockchainApp.Source.Common.Utils.UtilClasses.Menu;
using MoreLinq;
using Org.BouncyCastle.Math;
using Truncon.Collections;
using ContextMenu = BlockchainApp.Source.Common.Utils.UtilClasses.Menu.ContextMenu;

namespace BlockchainApp.Source.Common.Extensions.Collections
{
    public static class IEnumerableExtensions
    {
        #region - Converters

        public static BigInteger ToBigInt(this IEnumerable<byte> arr)
        {
            return new BigInteger(arr.ToArray());
        }

        public static byte[] HexToUTF8(this IEnumerable<byte> en) => en.ToArray().HexToUTF8();

        public static byte[] UTF8ToHex(this IEnumerable<byte> en) => en.ToArray().UTF8ToHex();

        public static string ToBase58String(this IEnumerable<byte> data)
        {
            var arrData = data.ToArray();
            return arrData.ToBase58String(0, arrData.Length);
        }

        public static OrderedDictionary<TKey, TValue> ToOrderedDictionary<TSource, TKey, TValue>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TValue> valueSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));

            var dictionary = comparer == null
                ? new OrderedDictionary<TKey, TValue>()
                : new OrderedDictionary<TKey, TValue>(comparer);

            foreach (var item in source)
                dictionary.Add(keySelector(item), valueSelector(item));

            return dictionary;
        }

        #endregion

        public static string ToStringDelimitedBy<T>(this IEnumerable<T> enumerable, string strBetween = "")
        {
            return string.Join(strBetween, enumerable);
        }

        public static string JoinAsString<T>(this IEnumerable<T> enumerable, string strBetween = "")
        {
            return enumerable.ToStringDelimitedBy(strBetween);
        }

        public static int Index<T>(this IEnumerable<T> en, T el)
        {
            var i = 0;
            foreach (var item in en)
            {
                if (Equals(item, el)) return i;
                i++;
            }
            return -1;
        }

        public static T LastOrNull<T>(this IEnumerable<T> enumerable)
        {
            var en = enumerable as T[] ?? enumerable.ToArray();
            return en.Any() ? en.Last() : (T)Convert.ChangeType(null, typeof(T));
        }

        public static IEnumerable<T> ConcatMany<T>(this IEnumerable<T> enumerable, params IEnumerable<T>[] enums)
        {
            return enumerable.Concat(enums.SelectMany(x => x));
        }

        public static IEnumerable<TSource> WhereByMany<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> selector,
            IEnumerable<TKey> matches) where TSource : class
        {
            return source.Where(e => matches.Any(sel => Equals(sel, selector(e))));
        }

        public static IEnumerable<TSource> OrderByWith<TSource, TResult>(this IEnumerable<TSource> en, Func<TSource, TResult> selector, IEnumerable<TResult> order)
        {
            return order.Select(el => en.Select(x => new { x, res = selector(x) }).Single(e => Equals(e.res, el)).x);
        }

        public static IEnumerable<T> OrderWith<T>(this IEnumerable<T> enumerable, IEnumerable<int> orderPattern)
        {
            var enArr = enumerable.ToArray();
            var opArr = orderPattern.ToArray();
            if (enArr.Length != opArr.Length)
                throw new ArgumentException($"{nameof(enumerable)}.Count() != {nameof(orderPattern)}.Count()");
            return opArr.Select(i => enArr[i]);
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, T el)
        {
            return enumerable.Except(new[] { el });
        }

        public static List<object> DisableControls(this IEnumerable<object> controls)
        {
            var disabledControls = new List<object>();
            foreach (var c in controls)
            {
                var piIsEnabled = c.GetType().GetProperty("IsEnabled");
                var isEnabled = (bool?)piIsEnabled?.GetValue(c);
                if (isEnabled == true)
                {
                    piIsEnabled.SetValue(c, false);
                    disabledControls.Add(c);
                }
            }
            return disabledControls;
        }

        public static void EnableControls(this IEnumerable<object> controls)
        {
            foreach (var c in controls)
            {
                var piIsEnabled = c.GetType().GetProperty("IsEnabled");
                piIsEnabled?.SetValue(c, true);

                if (c.GetType() == typeof(ContextMenu))
                {
                    var cm = (ContextMenu)c;
                    if (cm.IsOpen())
                    {
                        var wnd = cm.Control.LogicalAncestor<Window>();
                        var handler = wnd.GetType().GetRuntimeMethods().FirstOrDefault(m => m.Name == $"cm{cm.Control.Name.Take(1).ToUpper()}{cm.Control.Name.Skip(1)}_Open");
                        handler?.Invoke(wnd, new object[] { cm, new ContextMenuOpenEventArgs(cm) });
                    }
                }
            }
        }

        public static void ToggleControls(this IEnumerable<object> controls)
        {
            foreach (var c in controls)
                c.SetProperty("IsEnabled", c.GetProperty<bool>("IsEnabled"));
        }

        public static bool AllEqual<T>(this IEnumerable<T> en)
        {
            var arr = en.ToArray();
            return arr.All(el => Equals(el, arr.First()));
        }

        public static void Highlight<T>(this IEnumerable<T> controls, Color color) where T : Control
        {
            controls.ForEach(control => control.Highlight(color));
        }

        public static DataGridTextColumn ByDataMemberName(this IEnumerable<DataGridTextColumn> columns, string dataMemberName)
        {
            return columns.Single(c => String.Equals(c.DataMemberName(), dataMemberName, StringComparison.Ordinal));
        }

        public static NameValueCollection ToNameValueCollection(this IEnumerable<KeyValuePair<string, string>> en)
        {
            var nvc = new NameValueCollection();
            foreach (var q in en)
                nvc.Add(q.Key, q.Value);
            return nvc;
        }

        public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> en, bool distinct = true)
        {
            var duplicates = en.GroupBy(s => s).SelectMany(grp => grp.Skip(1));
            return distinct ? duplicates.Distinct() : duplicates;
        }

        public static List<TDest> MapCollectionTo<TDest, TSource>(this IEnumerable<TSource> source)
            where TSource : class
            where TDest : class
        {
            return source.Select(srcEl => srcEl.MapTo<TDest, TSource>()).ToList();
        }

        public static List<TDest> MapCollectionToSameType<TDest>(this IEnumerable<TDest> source) where TDest : class
        {
            return source.Select(srcEl => srcEl.MapToSameType()).ToList();
        }

        public static List<T> CopyCollection<T>(this IEnumerable<T> src) where T : class => src.MapCollectionToSameType();

        public static bool ContainsAll<T>(this IEnumerable<T> en1, IEnumerable<T> en2)
        {
            var arr1 = en1.Distinct().ToArray();
            var arr2 = en2.Distinct().ToArray();
            return arr1.Intersect(arr2).Count() == arr2.Length;
        }

        public static bool ContainsAny<T>(this IEnumerable<T> en1, IEnumerable<T> en2)
        {
            var arr1 = en1.Distinct().ToArray();
            var arr2 = en2.Distinct().ToArray();
            return arr1.Intersect(arr2).Any();
        }

        public static bool ContainsAll<T>(this IEnumerable<T> en1, params T[] en2)
        {
            return en1.ContainsAll(en2.AsEnumerable());
        }

        public static bool ContainsAny<T>(this IEnumerable<T> en1, params T[] en2)
        {
            return en1.ContainsAny(en2.AsEnumerable());
        }

        public static bool ContainsAll(this IEnumerable<string> en1, IEnumerable<string> en2)
        {
            var arr1 = en1.Select(x => x.ToLower()).Distinct().ToArray();
            var arr2 = en2.Select(x => x.ToLower()).Distinct().ToArray();
            return arr1.Intersect(arr2).Count() == arr2.Length;
        }

        public static bool ContainsAny(this IEnumerable<string> en1, IEnumerable<string> en2)
        {
            var arr1 = en1.Select(x => x.ToLower()).Distinct().ToArray();
            var arr2 = en2.Select(x => x.ToLower()).Distinct().ToArray();
            return arr1.Intersect(arr2).Any();
        }

        public static bool ContainsAll(this IEnumerable<string> en1, params string[] en2)
        {
            return en1.ContainsAll(en2.AsEnumerable());
        }

        public static bool ContainsAny(this IEnumerable<string> en1, params string[] en2)
        {
            return en1.ContainsAny(en2.AsEnumerable());
        }

        public static bool CollectionEqual<T>(this IEnumerable<T> col1, IEnumerable<T> col2)
        {
            if (col1 == null && col2 == null)
                return true;

            var arr1 = col1?.ToArray();
            var arr2 = col2.ToArray();
            return arr1?.Length == arr2.Length && arr1.Intersect(arr2).Count() == arr1.Length;
        }

        public static IEnumerable<TSource> ExceptBy<TSource, TSelector>(this IEnumerable<TSource> en, TSource el, Func<TSource, TSelector> selector)
        {
            return en.ExceptBy(el.ToEnumerable(), selector);
        }

        public static T FirstOrNull<T>(this IEnumerable<T> values) where T : class
        {
            return values.DefaultIfEmpty(null).FirstOrDefault();
        }

        public static T FirstOrNull<T>(this IEnumerable<T> values, Func<T, bool> selector) where T : class
        {
            var arrValues = values as T[] ?? values.ToArray();
            return arrValues.Any() ? arrValues.First(selector) : null;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> en)
        {
            return new ObservableCollection<T>(en);
        }

        public static void DetachFromParent(this IEnumerable<FrameworkElement> controls)
        {
            controls.ForEach(c => c.DetachFromParent());
        }

        public static void DetachFromParent(this FrameworkElement control)
        {
            ((Panel)control.Parent).Children.Remove(control);
        }

        public static string EnvVar(this string[] args, string var)
        {
            return args.FirstOrNull(a => a.StartsWith($"{var}="))?.AfterFirst($"{var}=");
        }

        public static IEnumerable<T> SliceExcl<T>(this IEnumerable<T> en, int startIndex, int endIndexExcluded)
        {
            return en.Skip(startIndex - 1).Take(endIndexExcluded - startIndex);
        }

        public static IEnumerable<T> Between<T>(this IEnumerable<T> en, int startIndex, int endIndexExcluded)
        {
            return en.Skip(startIndex - 1).Take(endIndexExcluded - startIndex + 1);
        }

        public static IEnumerable<T> AppendEl<T>(this IEnumerable<T> en, T el)
        {
            return MoreEnumerable.Append(en, el);
        }

        public static IEnumerable<T> PrependEl<T>(this IEnumerable<T> en, T el)
        {
            return MoreEnumerable.Prepend(en, el);
        }
    }
}
