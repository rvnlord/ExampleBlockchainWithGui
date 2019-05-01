using System.Collections.Generic;
using System.Text;
using Truncon.Collections;

namespace BlockchainApp.Source.Common.Extensions.Collections
{
    public static class OrderedDictionaryExtensions
    {
        public static TValue VorN<TKey, TValue>(this OrderedDictionary<TKey, TValue> dictionary, TKey key) where TValue : class
        {
            dictionary.TryGetValue(key, out var val);
            return val;
        }

        public static KeyValuePair<TKey, TValue> KVorN<TKey, TValue>(this OrderedDictionary<TKey, TValue> dictionary, TKey key) where TValue : class
        {
            dictionary.TryGetValue(key, out var val);
            return new KeyValuePair<TKey, TValue>(key, val);
        }

        public static string ToQueryString(this OrderedDictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count <= 0) return "";
            var sb = new StringBuilder();
            foreach (var item in parameters)
                sb.Append($"&{item.Key.ToUrlEncoded()}={item.Value.ToUrlEncoded()}");
            return sb.ToString().Skip(1);
        }
    }
}
