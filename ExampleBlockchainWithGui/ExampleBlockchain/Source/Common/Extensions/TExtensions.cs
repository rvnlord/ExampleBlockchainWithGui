using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BlockchainApp.Source.Common.Extensions.Collections;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class TExtensions
    {
        public static bool EqualsAny<T>(this T o, params T[] os)
        {
            return os.Length > 0 && os.Any(s => s.Equals(o));
        }

        public static bool EqualsAny<T>(this T o, IEnumerable<T> os)
        {
            var osArr = os.ToArray();
            return osArr.Length > 0 && osArr.Any(s => s.Equals(o));
        }

        public static bool EqualsAll<T>(this T o, params T[] os)
        {
            return os.Length > 0 && os.All(s => s.Equals(o));
        }

        public static T2 MapTo<T1, T2>(this T1 t1)
        {
            var t2 = (T2)Activator.CreateInstance(typeof(T2), new object[] { });
            Mapper.Map(t1, t2);
            return t2;
        }

        public static T NullifyIf<T>(this T o, Func<T, bool> condition)
        {
            if (condition(o))
                return (T)(object)null;
            return o;
        }

        public static T[] ToArrayOfOne<T>(this T el)
        {
            return new T[] { el };
        }

        public static IEnumerable<T> ConcatMany<T>(this T val, params IEnumerable<T>[] enums)
        {
            return IEnumerableExtensions.ConcatMany(new[] { val }, enums);
        }

        public static bool In<T>(this T o, params T[] os)
        {
            return o.EqualsAny(os);
        }

        public static TDest MapTo<TDest, TSource>(this TSource srcEl)
            where TSource : class
            where TDest : class
        {
            var destEl = (TDest)Activator.CreateInstance(typeof(TDest), new object[] { });
            Mapper.Map(srcEl, destEl);
            return destEl;
        }

        public static T MapToSameType<T>(this T srcEl) where T : class
        {
            var destEl = (T)Activator.CreateInstance(typeof(T), new object[] { });
            Mapper.Map(srcEl, destEl);
            return destEl;
        }

        public static T Copy<T>(this T src) where T : class => src.MapToSameType();

        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}
