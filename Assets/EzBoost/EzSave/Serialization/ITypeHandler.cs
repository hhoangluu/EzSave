using System;
using System.Collections.Generic;

namespace EzBoost.EzSave
{
    /// <summary>
    /// Interface for type handlers that can serialize and deserialize specific types
    /// </summary>
    public interface ITypeHandler
    {
        /// <summary>
        /// List of types supported by this handler
        /// </summary>
        IEnumerable<Type> SupportedTypes { get; }

        /// <summary>
        /// Check if this handler can handle a specific type
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if this handler can handle the type</returns>
        bool CanHandle(Type type);

        /// <summary>
        /// Serialize an object to a string
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>String representation of the object</returns>
        string Serialize(object obj);

        /// <summary>
        /// Deserialize a string to an object
        /// </summary>
        /// <param name="data">The string to deserialize</param>
        /// <param name="type">The target type</param>
        /// <returns>The deserialized object</returns>
        object Deserialize(string data, Type type);
    }
} 