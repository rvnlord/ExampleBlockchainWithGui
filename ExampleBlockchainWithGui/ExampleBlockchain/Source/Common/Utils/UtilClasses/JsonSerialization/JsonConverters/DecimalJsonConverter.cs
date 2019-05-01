using System;
using BlockchainApp.Source.Common.Extensions;
using Newtonsoft.Json;

namespace BlockchainApp.Source.Common.Utils.UtilClasses.JsonSerialization.JsonConverters
{
    public class DecimalJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue($"{(decimal)value:0.########}"); // json deserializer will break hashing because it may deserialize 100 as 1000.0 or 1000.00000000
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value.ToDecimal();
        }
    }
}
 