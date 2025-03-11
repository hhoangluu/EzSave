using System;
using System.Collections.Generic;
using UnityEngine;

namespace EzBoost.EzSave.Serialization.TypeHandlers
{
    /// <summary>
    /// Handles serialization of Dictionary types
    /// </summary>
    public class DictionaryHandler : ITypeHandler
    {
        [Serializable]
        private class SerializableDictionary
        {
            public string[] keys;
            public string[] values;
            public string[] types;
        }

        public IEnumerable<Type> SupportedTypes => new[] { typeof(Dictionary<string, object>) };

        public bool CanHandle(Type type)
        {
            return type == typeof(Dictionary<string, object>);
        }

        public string Serialize(object obj)
        {
            if (obj is not Dictionary<string, object> dictionary)
                throw new ArgumentException("Object must be Dictionary<string, object>");

            try
            {
                var serializableDict = new SerializableDictionary
                {
                    keys = new string[dictionary.Count],
                    values = new string[dictionary.Count],
                    types = new string[dictionary.Count]
                };

                int index = 0;
                foreach (var kvp in dictionary)
                {
                    serializableDict.keys[index] = kvp.Key;
                    
                    if (kvp.Value == null)
                    {
                        serializableDict.values[index] = "null";
                        serializableDict.types[index] = "null";
                    }
                    else
                    {
                        Type valueType = kvp.Value.GetType();
                        serializableDict.types[index] = valueType.AssemblyQualifiedName;
                        
                        // For primitive and simple types, serialize directly
                        if (valueType.IsPrimitive || valueType == typeof(string) || valueType == typeof(decimal))
                        {
                            serializableDict.values[index] = kvp.Value.ToString();
                        }
                        else
                        {
                            // For complex types, use JsonUtility
                            serializableDict.values[index] = JsonUtility.ToJson(kvp.Value);
                        }
                    }
                    
                    index++;
                }

                return JsonUtility.ToJson(serializableDict);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to serialize dictionary: {e.Message}");
                return "{}";
            }
        }

        public object Deserialize(string data, Type type)
        {
            if (type != typeof(Dictionary<string, object>))
                throw new ArgumentException("Type must be Dictionary<string, object>");

            var result = new Dictionary<string, object>();
            
            if (string.IsNullOrEmpty(data))
                return result;
                
            var serializableDict = JsonUtility.FromJson<SerializableDictionary>(data);
            
            if (serializableDict?.keys == null)
                return result;
                
            for (int i = 0; i < serializableDict.keys.Length; i++)
            {
                string key = serializableDict.keys[i];
                string value = serializableDict.values[i];
                string typeStr = serializableDict.types[i];
                
                if (typeStr == "null")
                {
                    result[key] = null;
                    continue;
                }
                
                Type valueType = Type.GetType(typeStr);
                
                if (valueType == null)
                {
                    Debug.LogWarning($"Could not find type '{typeStr}' during deserialization.");
                    continue;
                }
                
                try
                {
                    // For primitive and simple types
                    if (valueType == typeof(int))
                        result[key] = int.Parse(value);
                    else if (valueType == typeof(float))
                        result[key] = float.Parse(value);
                    else if (valueType == typeof(double))
                        result[key] = double.Parse(value);
                    else if (valueType == typeof(bool))
                        result[key] = bool.Parse(value);
                    else if (valueType == typeof(string))
                        result[key] = value;
                    else 
                    {
                        // For complex types, use JsonUtility
                        result[key] = JsonUtility.FromJson(value, valueType);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to deserialize value for key '{key}': {e.Message}");
                }
            }
            
            return result;
        }
    }
} 