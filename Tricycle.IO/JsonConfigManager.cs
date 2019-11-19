﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Tricycle.IO
{
    public class JsonConfigManager<T> : IConfigManager<T> where T: class, new()
    {
        static readonly JsonSerializerSettings SERIALIZER_SETTINGS = new JsonSerializerSettings
        {
            Converters = new JsonConverter[] { new StringEnumConverter(new CamelCaseNamingStrategy()) },
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        };

        IFileSystem _fileSystem;
        string _defaultFileName;
        string _userFileName;
        T _config;

        public JsonConfigManager(IFileSystem fileSystem, string defaultFileName, string userFileName)
        {
            _fileSystem = fileSystem;
            _defaultFileName = defaultFileName;
            _userFileName = userFileName;
        }

        public T Config
        {
            get => _config;
            set
            {
                if (!object.Equals(_config, value))
                {
                    _config = value;
                    ConfigChanged?.Invoke(value);
                }
            }
        }

        public event Action<T> ConfigChanged;

        public void Load()
        {
            T config = null;

            if (_fileSystem.File.Exists(_userFileName))
            {
                config = DeserializeFile(_userFileName);
            }

            if ((config == null) && _fileSystem.File.Exists(_defaultFileName))
            {
                config = DeserializeFile(_defaultFileName);

                if (config != null)
                {
                    SerializeToFile(config, _userFileName);
                }
            }

            Config = config ?? new T();
        }

        public void Save()
        {
            if (Config == null)
            {
                throw new InvalidOperationException("Config has not been loaded or set.");
            }

            SerializeToFile(Config, _userFileName);
        }

        T DeserializeFile(string fileName)
        {
            T result = null;

            try
            {
                string json = _fileSystem.File.ReadAllText(fileName);

                result = JsonConvert.DeserializeObject<T>(json, SERIALIZER_SETTINGS);
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine(ex);
            }

            return result;
        }

        void SerializeToFile(T obj, string fileName)
        {
            try
            {
                string json = JsonConvert.SerializeObject(obj, SERIALIZER_SETTINGS);

                _fileSystem.File.WriteAllText(fileName, json);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (NotSupportedException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (SecurityException ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}