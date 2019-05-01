using BlockchainApp.Source.Common.Utils.UtilClasses.JsonSerialization.JsonConverters;
using Newtonsoft.Json;

namespace BlockchainApp.Source.Common.Utils.UtilClasses.JsonSerialization
{
    public static class CustomJsonSerializer
    {
        private static JsonSerializer _s;

        public static JsonSerializer Serializer => _s ?? Get();

        private static JsonSerializer Get()
        {
            _s = new JsonSerializer
            {
                Formatting = Formatting.Indented
            };
            _s.Converters.Add(new UnixTimestampJsonConverter());
            _s.Converters.Add(new DecimalJsonConverter());
            return _s;
        }
    }
}
