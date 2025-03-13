using System;
using System.Collections.Generic;

namespace EzBoost.EzSave
{
        public interface ITypeHandler
    {
                IEnumerable<Type> SupportedTypes { get; }

                /// <param name="type">The type to check</param>
        /// <returns>True if this handler can handle the type</returns>
        bool CanHandle(Type type);

                /// <param name="obj">The object to serialize</param>
        /// <returns>String representation of the object</returns>
        string Serialize(object obj);

                /// <param name="data">The string to deserialize</param>
        /// <param name="type">The target type</param>
        /// <returns>The deserialized object</returns>
        object Deserialize(string data, Type type);
    }
} 