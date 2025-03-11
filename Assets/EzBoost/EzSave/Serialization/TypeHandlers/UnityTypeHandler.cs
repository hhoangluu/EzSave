using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace EzBoost.EzSave
{
    /// <summary>
    /// Handler for Unity-specific types (Vector2, Vector3, Quaternion, etc.)
    /// </summary>
    public class UnityTypeHandler : ITypeHandler
    {
        private static readonly Type[] _supportedTypes = new Type[]
        {
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(Quaternion),
            typeof(Color),
            typeof(Color32),
            typeof(Rect),
            typeof(Bounds)
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

            if (type == typeof(Vector2))
            {
                Vector2 v = (Vector2)obj;
                return $"{{\"x\":{v.x.ToString(CultureInfo.InvariantCulture)},\"y\":{v.y.ToString(CultureInfo.InvariantCulture)}}}";
            }
            else if (type == typeof(Vector3))
            {
                Vector3 v = (Vector3)obj;
                return $"{{\"x\":{v.x.ToString(CultureInfo.InvariantCulture)},\"y\":{v.y.ToString(CultureInfo.InvariantCulture)},\"z\":{v.z.ToString(CultureInfo.InvariantCulture)}}}";
            }
            else if (type == typeof(Vector4))
            {
                Vector4 v = (Vector4)obj;
                return $"{{\"x\":{v.x.ToString(CultureInfo.InvariantCulture)},\"y\":{v.y.ToString(CultureInfo.InvariantCulture)},\"z\":{v.z.ToString(CultureInfo.InvariantCulture)},\"w\":{v.w.ToString(CultureInfo.InvariantCulture)}}}";
            }
            else if (type == typeof(Quaternion))
            {
                Quaternion q = (Quaternion)obj;
                return $"{{\"x\":{q.x.ToString(CultureInfo.InvariantCulture)},\"y\":{q.y.ToString(CultureInfo.InvariantCulture)},\"z\":{q.z.ToString(CultureInfo.InvariantCulture)},\"w\":{q.w.ToString(CultureInfo.InvariantCulture)}}}";
            }
            else if (type == typeof(Color))
            {
                Color c = (Color)obj;
                return $"{{\"r\":{c.r.ToString(CultureInfo.InvariantCulture)},\"g\":{c.g.ToString(CultureInfo.InvariantCulture)},\"b\":{c.b.ToString(CultureInfo.InvariantCulture)},\"a\":{c.a.ToString(CultureInfo.InvariantCulture)}}}";
            }
            else if (type == typeof(Color32))
            {
                Color32 c = (Color32)obj;
                return $"{{\"r\":{c.r},\"g\":{c.g},\"b\":{c.b},\"a\":{c.a}}}";
            }
            else if (type == typeof(Rect))
            {
                Rect r = (Rect)obj;
                return $"{{\"x\":{r.x.ToString(CultureInfo.InvariantCulture)},\"y\":{r.y.ToString(CultureInfo.InvariantCulture)},\"width\":{r.width.ToString(CultureInfo.InvariantCulture)},\"height\":{r.height.ToString(CultureInfo.InvariantCulture)}}}";
            }
            else if (type == typeof(Bounds))
            {
                Bounds b = (Bounds)obj;
                Vector3 c = b.center;
                Vector3 s = b.size;
                return $"{{\"center\":{{\"x\":{c.x.ToString(CultureInfo.InvariantCulture)},\"y\":{c.y.ToString(CultureInfo.InvariantCulture)},\"z\":{c.z.ToString(CultureInfo.InvariantCulture)}}},\"size\":{{\"x\":{s.x.ToString(CultureInfo.InvariantCulture)},\"y\":{s.y.ToString(CultureInfo.InvariantCulture)},\"z\":{s.z.ToString(CultureInfo.InvariantCulture)}}}}}";
            }

            return "null";
        }

        public object Deserialize(string data, Type type)
        {
            try
            {
                if (data == "null")
                    return null;

                // For this basic implementation, we'll use Unity's built-in JSON utility
                // In the future, we'll implement more robust parsing
                if (type == typeof(Vector2))
                {
                    return JsonUtility.FromJson<Vector2>(data);
                }
                else if (type == typeof(Vector3))
                {
                    return JsonUtility.FromJson<Vector3>(data);
                }
                else if (type == typeof(Vector4))
                {
                    return JsonUtility.FromJson<Vector4>(data);
                }
                else if (type == typeof(Quaternion))
                {
                    return JsonUtility.FromJson<Quaternion>(data);
                }
                else if (type == typeof(Color))
                {
                    return JsonUtility.FromJson<Color>(data);
                }
                else if (type == typeof(Color32))
                {
                    return JsonUtility.FromJson<Color32>(data);
                }
                else if (type == typeof(Rect))
                {
                    return JsonUtility.FromJson<Rect>(data);
                }
                else if (type == typeof(Bounds))
                {
                    return JsonUtility.FromJson<Bounds>(data);
                }

                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to deserialize Unity type data. Error: {e.Message}");
                return type.IsValueType ? Activator.CreateInstance(type) : null;
            }
        }
    }
} 