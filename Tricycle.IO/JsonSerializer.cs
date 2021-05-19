using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Tricycle.IO
{
    public class JsonSerializer : ISerializer<string>
    {
        JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Converters = new JsonConverter[]
            {
                new StringEnumConverter(new CamelCaseNamingStrategy()),
                new VersionConverter()
            },
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
                {
                    ProcessDictionaryKeys = false
                }
            },
            Formatting = Formatting.Indented
        };

        public JsonSerializer()
        {

        }

        public JsonSerializer(JsonSerializerSettings settings)
        {
            _settings = settings;
        }

        #region ISerializer Methods

        public TObject Deserialize<TObject>(string data)
        {
            try
            {
                return JsonConvert.DeserializeObject<TObject>(data, _settings);
            }
            catch (JsonException ex)
            {
                throw new SerializationException("An error occurred deserializing the data.", ex);
            }
        }

        public string Serialize(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, _settings);
            }
            catch (JsonException ex)
            {
                throw new SerializationException("An error occurred serializing the object.", ex);
            }
        }

        #endregion
    }
}
