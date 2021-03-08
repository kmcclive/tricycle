using System;
using System.Runtime.Serialization;

namespace Tricycle.IO
{
    /// <summary>
    /// Supports serialization and deserialzation.
    /// </summary>
    /// <typeparam name="T">The type to serialize to and deserialize from.</typeparam>
    public interface ISerializer<T>
    {
        /// <summary>
        /// Serializes a specified object.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>The serialized data.</returns>
        /// <exception cref="SerializationException">An error occurred serializing the object.</exception>
        T Serialize(object obj);

        /// <summary>
        /// Deserializes specified data.
        /// </summary>
        /// <typeparam name="TObject">The type to deserialize to.</typeparam>
        /// <param name="data">The data to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        /// <exception cref="SerializationException">An error occurred deserializing the data.</exception>
        TObject Deserialize<TObject>(T data);
    }
}
