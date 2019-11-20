using System;
namespace Tricycle.IO
{
    public interface IConfigManager<T>
    {
        /// <summary>
        /// The current config.
        /// </summary>
        T Config { get; set; }

        /// <summary>
        /// Occurs when the config changes.
        /// </summary>
        event Action<T> ConfigChanged;

        /// <summary>
        /// Loads a config and updates the current config.
        /// </summary>
        void Load();

        /// <summary>
        /// Saves the current config.
        /// </summary>
        /// <exception cref="InvalidOperationException">Config has not been loaded or set.</exception>
        void Save();
    }
}
