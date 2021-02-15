using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Security;
using Newtonsoft.Json;

namespace Tricycle.IO
{
    public class FileConfigManager<T> : IConfigManager<T> where T: class, new()
    {
        IFileSystem _fileSystem;
        ISerializer<string> _serializer;
        string _defaultFileName;
        string _userFileName;
        T _defaultConfig;
        T _config;

        public FileConfigManager(IFileSystem fileSystem,
                                 ISerializer<string> serializer,
                                 string defaultFileName,
                                 string userFileName)
        {
            _fileSystem = fileSystem;
            _serializer = serializer;
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

                    if (_config != null && _defaultConfig != null)
                    {
                        Coalesce(_config, _defaultConfig);
                    }

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

            if (_fileSystem.File.Exists(_defaultFileName))
            {
                _defaultConfig = DeserializeFile(_defaultFileName);

                if (_defaultConfig != null)
                {
                    if (config != null)
                    {
                        Coalesce(config, _defaultConfig);
                    }
                    else
                    {
                        config = _defaultConfig;
                    }

                    SerializeToFile(config, _userFileName);
                }
            }

            _config = config ?? new T();
            ConfigChanged?.Invoke(_config);
        }

        public void Save()
        {
            if (Config == null)
            {
                throw new InvalidOperationException("Config has not been loaded or set.");
            }

            SerializeToFile(Config, _userFileName);
        }

        protected virtual void Coalesce(T userConfig, T defaultConfig)
        {

        }

        T DeserializeFile(string fileName)
        {
            T result = null;

            try
            {
                string json = _fileSystem.File.ReadAllText(fileName);

                result = _serializer.Deserialize<T>(json);
            }
            catch (IOException ex)
            {
                Trace.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
            catch (JsonException ex)
            {
                Trace.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return result;
        }

        void SerializeToFile(T obj, string fileName)
        {
            try
            {
                string directory = Path.GetDirectoryName(fileName);

                if (!_fileSystem.Directory.Exists(directory))
                {
                    _fileSystem.Directory.CreateDirectory(directory);
                }

                string json = _serializer.Serialize(obj);

                _fileSystem.File.WriteAllText(fileName, json);
            }
            catch (JsonException ex)
            {
                Trace.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
            catch (IOException ex)
            {
                Trace.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
            catch (NotSupportedException ex)
            {
                Trace.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
            catch (UnauthorizedAccessException ex)
            {
                Trace.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
            catch (SecurityException ex)
            {
                Trace.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }
}
