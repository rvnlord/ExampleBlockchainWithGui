using System;
using System.Collections.Generic;
using BlockchainApp.Source.Common.Utils.UtilClasses.JsonSerialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static BlockchainApp.Source.Config;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class JTokenExtensions
    {
        public static JObject ToJObject(this JToken jToken)
        {
            return (JObject)jToken;
        }

        public static JArray ToJArray(this JToken jToken)
        {
            return (JArray)jToken;
        }

        public static bool IsNullOrEmpty(this JToken jToken)
        {
            return (jToken == null) ||
                   (jToken.Type == JTokenType.Array && !jToken.HasValues) ||
                   (jToken.Type == JTokenType.Object && !jToken.HasValues) ||
                   (jToken.Type == JTokenType.String && jToken.ToString() == string.Empty) ||
                   (jToken.Type == JTokenType.Null);
        }

        public static string ToStringN(this JToken jToken)
        {
            return jToken.IsNullOrEmpty() ? null : jToken.ToString();
        }

        public static T To<T>(this JToken jToken)
        {
            T o;
            try
            {
                o = !jToken.IsNullOrEmpty()
                    ? jToken.ToObject<T>(CustomJsonSerializer.Serializer)
                    : default;
            }
            catch (JsonSerializationException ex)
            {
                _logger.Error(ex);
                return (T) (object) null;
            }

            if (o == null && typeof(T).IsIListType() && typeof(T).IsGenericType)
            {
                var elT = typeof(T).GetGenericArguments()[0];
                var listType = typeof(List<>);
                var constructedListType = listType.MakeGenericType(elT);
                return (T) Activator.CreateInstance(constructedListType);
            }
            if (o == null && typeof(T).IsIDictionaryType() && typeof(T).IsGenericType)
            {
                var keyT = typeof(T).GetGenericArguments()[0];
                var valT = typeof(T).GetGenericArguments()[1];
                var dictType = typeof(Dictionary<,>);
                var constructedDictType = dictType.MakeGenericType(keyT, valT);
                return (T)Activator.CreateInstance(constructedDictType);
            }

            return o;
        }

        public static bool DeepEquals(this JToken jToken1, JToken jToken2)
        {
            return JToken.DeepEquals(jToken1, jToken2);
        }
    }
}
