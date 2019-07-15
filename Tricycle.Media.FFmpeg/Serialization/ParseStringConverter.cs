using System;
using Newtonsoft.Json;

namespace Tricycle.Media.FFmpeg.Serialization
{
    //Adapted from https://app.quicktype.io/
    class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(long) || objectType == typeof(long?);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (long.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var typedValue = (long)value;
            serializer.Serialize(writer, typedValue.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
