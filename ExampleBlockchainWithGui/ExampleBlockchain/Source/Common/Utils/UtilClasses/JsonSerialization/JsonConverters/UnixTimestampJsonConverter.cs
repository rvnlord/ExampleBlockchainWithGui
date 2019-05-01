using System;
using BlockchainApp.Source.Common.Extensions;
using Newtonsoft.Json;

namespace BlockchainApp.Source.Common.Utils.UtilClasses.JsonSerialization.JsonConverters
{
    public class UnixTimestampJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UnixTimestamp);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((UnixTimestamp)value).ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value?.ToDouble().ToUnixTimestamp();
        }
    }
}
