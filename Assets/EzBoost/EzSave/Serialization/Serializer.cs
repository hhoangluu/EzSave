using System;
using System.Collections.Generic;
using UnityEngine;
using EzBoost.EzSave.Serialization.TypeHandlers;

namespace EzBoost.EzSave.Serialization
{
        public class Serializer
    {
        // Dictionary to store custom type handlers
        private readonly Dictionary<Type, ITypeHandler> _typeHandlers;

        [Serializable]
        private class SerializableDictionary
        {
            public string[] keys;
            public string[] values;
            public string[] types;
        }

        public Serializer()
        {
            _typeHandlers = new Dictionary<Type, ITypeHandler>();
            RegisterDefaultHandlers();
        }

                private void RegisterDefaultHandlers()
        {
            RegisterTypeHandler(new PrimitiveHandler());
            RegisterTypeHandler(new UnityTypeHandler());
            RegisterTypeHandler(new DictionaryHandler());
        }

                /// <param name="handler">The type handler to register</param>
        public void RegisterTypeHandler(ITypeHandler handler)
        {
            foreach (Type type in handler.SupportedTypes)
            {
                _typeHandlers[type] = handler;
            }
        }

                /// <param name="obj">The object to serialize</param>
        /// <returns>JSON string representation of the object</returns>
        public string Serialize<T>(T obj)
        {
            try
            {
                Type type = typeof(T);
                
                // Check if we have a custom handler for this type
                if (_typeHandlers.TryGetValue(type, out ITypeHandler handler))
                {
                    return handler.Serialize(obj);
                }
                
                // Make sure the object is serializable
                if (!type.IsSerializable && !type.IsDefined(typeof(SerializableAttribute), false))
                {
                    Debug.LogWarning($"Type {type.Name} is not marked as [Serializable]");
                }
                
                string json = JsonUtility.ToJson(obj);
                
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError($"JsonUtility.ToJson returned empty string for type {type.Name}");
                }
                
                return json;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to serialize object of type {typeof(T).Name}. Error: {e.Message}");
                return string.Empty;
            }
        }

                /// <param name="json">The JSON string to deserialize</param>
        /// <returns>The deserialized object</returns>
        public T Deserialize<T>(string json)
        {
            try
            {
                Type type = typeof(T);
                
                // Check if we have a custom handler for this type
                if (_typeHandlers.TryGetValue(type, out ITypeHandler handler))
                {
                    return (T)handler.Deserialize(json, type);
                }
                
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize JSON to type {typeof(T).Name}. Error: {e.Message}");
                return default;
            }
        }
    }
} 