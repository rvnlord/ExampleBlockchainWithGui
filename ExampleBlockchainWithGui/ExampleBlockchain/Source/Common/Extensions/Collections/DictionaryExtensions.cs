using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using static BlockchainApp.Source.Common.Constants;
using static BlockchainApp.Source.Common.Utils.LockUtils;

namespace BlockchainApp.Source.Common.Extensions.Collections
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out var value) ? value : default;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueProvider)
        {
            return dictionary.TryGetValue(key, out var value) ? value
                : defaultValueProvider();
        }

        public static TValue VorDef<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return GetValueOrDefault(dictionary, key);
        }

        public static TValue VorDef<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            return GetValueOrDefault(dictionary, key, defaultValue);
        }

        public static TValue VorDef<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueProvider)
        {
            return GetValueOrDefault(dictionary, key, defaultValueProvider);
        }

        public static TValue GetValueOrNull<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.TryGetValue(key, out var val);
            return val;
        }

        public static TValue VorN<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : class
        {
            return GetValueOrNull(dictionary, key);
        }

        public static KeyValuePair<TKey, TValue> KVorN<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : class
        {
            dictionary.TryGetValue(key, out var val);
            return new KeyValuePair<TKey, TValue>(key, val);
        }

        public static void AddIfNotNull<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TValue : class
        {
            if (value != null)
                dictionary.Add(key, value);
        }

        public static void AddIfNotNullAnd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, bool? condition, TKey key, TValue value) where TValue : class
        {
            if (value != null && condition == true)
                dictionary.Add(key, value);
        }

        public static string ToQueryString(this Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count <= 0) return "";
            var sb = new StringBuilder();
            foreach (var item in parameters)
                sb.Append($"&{item.Key.ToUrlEncoded()}={item.Value.ToUrlEncoded()}");
            return sb.ToString().Skip(1);
        }

        public static bool Exists<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : class
        {
            return dict.VorN(key) != null;
        }

        public static void RemoveIfExists<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : class
        {
            if (dict.Exists(key))
                dict.Remove(key);
        }

        public static TValue VorN_Ts<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            return Lock (_globalSync, nameof(_globalSync), nameof(VorN_Ts), () =>
                GetValueOrNull(dictionary, key));
        }

        public static void V_Ts<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue val)
        {
            Lock (_globalSync, nameof(_globalSync), nameof(V_Ts), () =>
               dictionary[key] = val);
        }
    }
}
