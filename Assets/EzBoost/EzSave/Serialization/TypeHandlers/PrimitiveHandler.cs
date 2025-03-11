using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace EzBoost.EzSave
{
    /// <summary>
    /// Handler for primitive C# types (int, float, string, bool, etc.)
    /// </summary>
    public class PrimitiveHandler : ITypeHandler
    {
        private static readonly Type[] _supportedTypes = new Type[]
        {
            typeof(int),
            typeof(float),
            typeof(double),
            typeof(bool),
            typeof(string),
            typeof(char),
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(decimal),
            typeof(DateTime)
        };

        public IEnumerable<Type> SupportedTypes => _supportedTypes;

        public bool CanHandle(Type type)
        {
            return Array.IndexOf(_supportedTypes, type) >= 0;
        }

        public string Serialize(object obj)
        {
            if (obj == null)
                return "null";

            Type type = obj.GetType();

            // Handle different primitive types
            if (type == typeof(string))
            {
                // Escape quotes in strings
                return "\"" + ((string)obj).Replace("\"", "\\\"") + "\"";
            }
            else if (type == typeof(char))
            {
                return "\"" + obj + "\"";
            }
            else if (type == typeof(bool))
            {
                return (bool)obj ? "true" : "false";
            }
            else if (type == typeof(DateTime))
            {
                return "\"" + ((DateTime)obj).ToString("o", CultureInfo.InvariantCulture) + "\"";
            }
            else
            {
                // For numbers, just use ToString
                return Convert.ToString(obj, CultureInfo.InvariantCulture);
            }
        }

        public object Deserialize(string data, Type type)
        {
            try
            {
                if (data == "null")
                    return null;

                // Handle different primitive types
                if (type == typeof(string))
                {
                    // Remove quotes from strings
                    if (data.StartsWith("\"") && data.EndsWith("\""))
                        return data.Substring(1, data.Length - 2).Replace("\\\"", "\"");
                    return data;
                }
                else if (type == typeof(char))
                {
                    if (data.StartsWith("\"") && data.EndsWith("\"") && data.Length == 3)
                        return data[1];
                    return '\0';
                }
                else if (type == typeof(bool))
                {
                    return data.ToLower() == "true";
                }
                else if (type == typeof(int))
                {
                    return int.Parse(data, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(float))
                {
                    return float.Parse(data, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(double))
                {
                    return double.Parse(data, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(decimal))
                {
                    return decimal.Parse(data, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(byte))
                {
                    return byte.Parse(data, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(sbyte))
                {
                    return sbyte.Parse(data, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(short))
                {
                    return short.Parse(data, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(ushort))
                {
                    return ushort.Parse(data, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(uint))
                {
                    return uint.Parse(data, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(long))
                {
                    return long.Parse(data, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(ulong))
                {
                    return ulong.Parse(data, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(DateTime))
                {
                    if (data.StartsWith("\"") && data.EndsWith("\""))
                        data = data.Substring(1, data.Length - 2);
                    return DateTime.Parse(data, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                }

                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to deserialize primitive data. Error: {e.Message}");
                return type.IsValueType ? Activator.CreateInstance(type) : null;
            }
        }
    }
} 