using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using EzBoost.EzSave.Serialization;
using System.Threading.Tasks;
using EzBoost.EzSave.Crypto;
using EzBoost.EzSave.Storage;
using EzBoost.EzSave;

namespace EzBoost.EzSave.Core
{
        internal static class EzSaveCore
    {
        private static readonly Serializer _serializer = new Serializer();
        private const string DEFAULT_FILENAME = "save.json";

                private static SaveSettings GetDefaultSettings()
        {
            // Try to load default settings from the ScriptableObject in Resources
            var defaultSettingAsset = Resources.Load<EzSaveDefaultSetting>("EzSaveDefaultSetting");
            return defaultSettingAsset.defaultSaveSetting;
        }

                internal static string GetFilePath(SaveSettings settings)
        {
            settings = settings ?? GetDefaultSettings();
            string fileName = settings.SubFolder != null ? 
                System.IO.Path.Combine(settings.SubFolder, settings.FileName) : 
                settings.FileName;
            return EzSaveIO.GetFilePath(fileName, settings);
        }

        #region Save/Load Operations
                internal static bool Save<T>(string key, T data, SaveSettings settings)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);
                var saveData = LoadSaveData(settings);

                // Add or update the data in the dictionary
                if (saveData.ContainsKey(key))
                    saveData[key] = data;
                else
                    saveData.Add(key, data);

                return SaveDictionary(saveData, settings);
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to save data to {settings?.FileName ?? DEFAULT_FILENAME} with key {key}. Error: {e.Message}");
                return false;
            }
        }

                internal static T Load<T>(string key, SaveSettings settings)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);
                
                if (!EzSaveIO.FileExists(filePath, settings))
                {
                    Debug.LogWarning($"EzSave: Save file {settings.FileName} does not exist");
                    return default;
                }

                var saveData = LoadSaveData(settings);
                
                if (!saveData.ContainsKey(key))
                {
                    Debug.LogWarning($"EzSave: Key '{key}' not found in file {settings.FileName}");
                    return default;
                }

                return (T)saveData[key];
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to load data from {settings?.FileName ?? DEFAULT_FILENAME} with key {key}. Error: {e.Message}");
                return default;
            }
        }
        #endregion

        #region Dictionary Operations
                internal static Dictionary<string, object> LoadSaveData(SaveSettings settings = null)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);

                if (!EzSaveIO.FileExists(filePath, settings))
                {
                    return new Dictionary<string, object>();
                }

                string jsonData = EzSaveIO.ReadFile(filePath, settings);
                
                if (string.IsNullOrEmpty(jsonData))
                {
                    return new Dictionary<string, object>();
                }

                // Apply decryption if needed
                if (settings.EncryptionType != EncryptionType.None)
                {
                    jsonData = EzSaveCrypto.Decrypt(jsonData, settings);
                }

                // Apply decompression if enabled
                if (settings.UseCompression)
                {
                    // Decompression will be implemented in future steps
                    Debug.LogWarning("EzSave: Decompression is not yet implemented.");
                }

                var saveData = _serializer.Deserialize<Dictionary<string, object>>(jsonData);
                return saveData ?? new Dictionary<string, object>();
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to load save data from {settings?.FileName ?? DEFAULT_FILENAME}. Error: {e.Message}");
                return new Dictionary<string, object>();
            }
        }

                internal static bool SaveDictionary(Dictionary<string, object> dictionary, SaveSettings settings)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);
                string jsonData = _serializer.Serialize(dictionary);

                // Apply compression if enabled
                if (settings.UseCompression)
                {
                    // Compression will be implemented in future steps
                    Debug.LogWarning("EzSave: Compression is not yet implemented.");
                }

                // Apply encryption if needed
                if (settings.EncryptionType != EncryptionType.None)
                {
                    jsonData = EzSaveCrypto.Encrypt(jsonData, settings);
                }

                EzSaveIO.WriteFile(filePath, jsonData, settings);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to save dictionary to {settings?.FileName ?? DEFAULT_FILENAME}. Error: {e.Message}");
                return false;
            }
        }
        #endregion

        #region Key Operations
                internal static bool KeyExists(string key, SaveSettings settings)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                var saveData = LoadSaveData(settings);
                return saveData.ContainsKey(key);
            }
            catch
            {
                return false;
            }
        }

                internal static bool DeleteKey(string key, SaveSettings settings)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);
                
                if (!EzSaveIO.FileExists(filePath, settings))
                    return false;

                var saveData = LoadSaveData(settings);
                
                if (!saveData.ContainsKey(key))
                    return false;

                saveData.Remove(key);
                
                // Only write back if there are still entries
                if (saveData.Count > 0)
                {
                    return SaveDictionary(saveData, settings);
                }
                
                // Delete the file if no entries remain
                return EzSaveIO.DeleteFile(filePath, settings);
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to delete key '{key}' from {settings?.FileName ?? DEFAULT_FILENAME}. Error: {e.Message}");
                return false;
            }
        }

                internal static string[] GetKeys(SaveSettings settings = null)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);
                
                if (!EzSaveIO.FileExists(filePath))
                    return new string[0];

                var saveData = LoadSaveData(settings);
                return saveData.Keys.ToArray();
            }
            catch
            {
                return new string[0];
            }
        }
        #endregion

        #region Data Management
                internal static bool DeleteAllData()
        {
            try
            {
                // Any cleanup of encryption/compression resources would go here
                return EzSaveIO.DeleteAllFiles();
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to delete all data. Error: {e.Message}");
                return false;
            }
        }
        #endregion

        #region File Management
                internal static bool FileExists(SaveSettings settings)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);
                return EzSaveIO.FileExists(filePath, settings);
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to check if file exists {settings?.FileName ?? DEFAULT_FILENAME}. Error: {e.Message}");
                return false;
            }
        }

                internal static bool DeleteFile(SaveSettings settings)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);
                return EzSaveIO.DeleteFile(filePath, settings);
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to delete file {settings?.FileName ?? DEFAULT_FILENAME}. Error: {e.Message}");
                return false;
            }
        }
        #endregion

        #region Async Save/Load Operations
                internal static async Task<bool> SaveAsync<T>(string key, T data, SaveSettings settings)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);
                var saveData = await LoadSaveDataAsync(settings);

                // Add or update the data in the dictionary
                if (saveData.ContainsKey(key))
                    saveData[key] = data;
                else
                    saveData.Add(key, data);

                return await SaveDictionaryAsync(saveData, settings);
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to save data to {settings?.FileName ?? DEFAULT_FILENAME} with key {key} asynchronously. Error: {e.Message}");
                return false;
            }
        }

                internal static async Task<T> LoadAsync<T>(string key, SaveSettings settings)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);
                
                if (!EzSaveIO.FileExists(filePath))
                {
                    Debug.LogWarning($"EzSave: Save file {settings.FileName} does not exist");
                    return default;
                }

                var saveData = await LoadSaveDataAsync(settings);
                
                if (!saveData.ContainsKey(key))
                {
                    Debug.LogWarning($"EzSave: Key '{key}' not found in file {settings.FileName}");
                    return default;
                }

                return (T)saveData[key];
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to load data from {settings?.FileName ?? DEFAULT_FILENAME} with key {key} asynchronously. Error: {e.Message}");
                return default;
            }
        }
        #endregion

        #region Async Dictionary Operations
                internal static async Task<Dictionary<string, object>> LoadSaveDataAsync(SaveSettings settings = null)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);
                
                if (!EzSaveIO.FileExists(filePath, settings))
                {
                    return new Dictionary<string, object>();
                }

                string jsonData = await EzSaveIO.ReadFileAsync(filePath, settings);
                
                if (string.IsNullOrEmpty(jsonData))
                {
                    return new Dictionary<string, object>();
                }

                // Apply decryption if needed
                if (settings.EncryptionType != EncryptionType.None)
                {
                    jsonData = EzSaveCrypto.Decrypt(jsonData, settings);
                }

                // Apply decompression if enabled
                if (settings.UseCompression)
                {
                    // Decompression will be implemented in future steps
                    Debug.LogWarning("EzSave: Decompression is not yet implemented.");
                }

                var saveData = _serializer.Deserialize<Dictionary<string, object>>(jsonData);
                return saveData ?? new Dictionary<string, object>();
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to load save data from {settings?.FileName ?? DEFAULT_FILENAME} asynchronously. Error: {e.Message}");
                return new Dictionary<string, object>();
            }
        }

                internal static async Task<bool> SaveDictionaryAsync(Dictionary<string, object> dictionary, SaveSettings settings)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);
                string jsonData = _serializer.Serialize(dictionary);

                // Apply compression if enabled
                if (settings.UseCompression)
                {
                    // Compression will be implemented in future steps
                    Debug.LogWarning("EzSave: Compression is not yet implemented.");
                }

                // Apply encryption if needed
                if (settings.EncryptionType != EncryptionType.None)
                {
                    jsonData = EzSaveCrypto.Encrypt(jsonData, settings);
                }

                await EzSaveIO.WriteFileAsync(filePath, jsonData, settings);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to save dictionary to {settings?.FileName ?? DEFAULT_FILENAME} asynchronously. Error: {e.Message}");
                return false;
            }
        }
        #endregion

        #region Async Key Operations
                internal static async Task<bool> DeleteKeyAsync(string key, SaveSettings settings)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);
                
                if (!EzSaveIO.FileExists(filePath, settings))
                    return false;

                var saveData = await LoadSaveDataAsync(settings);
                
                if (!saveData.ContainsKey(key))
                    return false;

                saveData.Remove(key);
                
                // Only write back if there are still entries
                if (saveData.Count > 0)
                {
                    return await SaveDictionaryAsync(saveData, settings);
                }
                
                // Delete the file if no entries remain
                return await EzSaveIO.DeleteFileAsync(filePath, settings);
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to delete key '{key}' from {settings?.FileName ?? DEFAULT_FILENAME} asynchronously. Error: {e.Message}");
                return false;
            }
        }
        #endregion

        #region Async File Management
                internal static async Task<bool> DeleteFileAsync(SaveSettings settings)
        {
            try
            {
                settings = settings ?? GetDefaultSettings();
                string filePath = GetFilePath(settings);
                return await EzSaveIO.DeleteFileAsync(filePath, settings);
            }
            catch (Exception e)
            {
                Debug.LogError($"EzSave: Failed to delete file {settings?.FileName ?? DEFAULT_FILENAME} asynchronously. Error: {e.Message}");
                return false;
            }
        }
        #endregion
    }
} 