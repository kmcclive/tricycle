using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Tricycle.IO
{
    public class JsonSerializer : ISerializer<string>
    {
        JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Converters = new JsonConverter[] { new StringEnumConverter(new CamelCaseNamingStrategy()) },
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
            return JsonConvert.DeserializeObject<TObject>(data, _settings);
        }

        public string Seriialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _settings);
        }

        #endregion
    }
}
