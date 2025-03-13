using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using EzBoost.EzSave.Core;
using System.Threading.Tasks;

namespace EzBoost.EzSave
{
    /// <summary>
    /// Save operation that provides both a Task and an event for completion notification
    /// </summary>
    public class SaveOperation
    {
        private readonly Task<bool> _task;
        
        /// <summary>
        /// Event that fires when the save operation completes
        /// </summary>
        public event Action<bool> OnComplete;
        
        /// <summary>
        /// Task representing the async operation
        /// </summary>
        public Task<bool> Task => _task;
        
        internal SaveOperation(Task<bool> task)
        {
            _task = task;
            
            // When the task completes, invoke the OnComplete event on the main thread
            _task.ContinueWith(t => {
                if (OnComplete != null)
                {
                    Core.EzSaveAsync.Instance().Enqueue(() => OnComplete(t.Result));
                }
            });
        }
    }
    
    /// <summary>
    /// Load operation that provides both a Task and an event for completion notification
    /// </summary>
    public class LoadOperation<T>
    {
        private readonly Task<T> _task;
        
        /// <summary>
        /// Event that fires when the load operation completes
        /// </summary>
        public event Action<T> OnComplete;
        
        /// <summary>
        /// Task representing the async operation
        /// </summary>
        public Task<T> Task => _task;
        
        internal LoadOperation(Task<T> task)
        {
            _task = task;
            
            // When the task completes, invoke the OnComplete event on the main thread
            _task.ContinueWith(t => {
                if (OnComplete != null)
                {
                    Core.EzSaveAsync.Instance().Enqueue(() => OnComplete(t.Result));
                }
            });
        }
    }

    /// <summary>
    /// Main entry point for the EzSave plugin.
    /// Provides simple methods to save and load data to/from files.
    /// </summary>
    public static class EzSave
    {
        /// <summary>
        /// Initialize the EzSave plugin
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Debug.Log("EzSave: Initializing...");
            EzSaveIO.Initialize();
            
            // Initialize the async handler
            Core.EzSaveAsync.Instance();
            
            Debug.Log("EzSave: Initialization complete. Save path: " + EzSaveIO.GetSavePath());
        }

        #region Save Methods
        /// <summary>
        /// Save data with the specified key using default settings
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="data">The data to save</param>
        /// <returns>True if save was successful</returns>
        public static bool Save<T>(string key, T data)
        {
            return Save(key, data, (SaveSettings)null);
        }

        /// <summary>
        /// Save data with the specified key to the specified file
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="data">The data to save</param>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>True if save was successful</returns>
        public static bool Save<T>(string key, T data, string fileName)
        {
            return Save(key, data, new SaveSettings(fileName));
        }

        /// <summary>
        /// Save data with the specified key using custom settings
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="data">The data to save</param>
        /// <param name="settings">Custom save settings (if null, default settings with "save.json" will be used)</param>
        /// <returns>True if save was successful</returns>
        public static bool Save<T>(string key, T data, SaveSettings settings = null)
        {
           
            
            return EzSaveCore.Save(key, data, settings);
        }
        #endregion

        #region Load Methods
        /// <summary>
        /// Load data with the specified key using default settings
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <returns>The loaded data or default if not found</returns>
        public static T Load<T>(string key)
        {
            return Load<T>(key, (SaveSettings)null);
        }

        /// <summary>
        /// Load data with the specified key from the specified file
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>The loaded data or default if not found</returns>
        public static T Load<T>(string key, string fileName)
        {
            return Load<T>(key, new SaveSettings(fileName));
        }

        /// <summary>
        /// Load data with the specified key using custom settings
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="settings">Custom save settings (if null, default settings with "save.json" will be used)</param>
        /// <returns>The loaded data or default if not found</returns>
        public static T Load<T>(string key, SaveSettings settings = null)
        {
           

            return EzSaveCore.Load<T>(key, settings);
        }

        /// <summary>
        /// Load data with the specified key using default settings, returning the defaultValue if not found
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="defaultValue">The value to return if the key doesn't exist</param>
        /// <returns>The loaded data or defaultValue if not found</returns>
        public static T Load<T>(string key, T defaultValue)
        {
            return Load<T>(key, defaultValue, (SaveSettings)null);
        }

        /// <summary>
        /// Load data with the specified key from the specified file, returning the defaultValue if not found
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="defaultValue">The value to return if the key doesn't exist</param>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>The loaded data or defaultValue if not found</returns>
        public static T Load<T>(string key, T defaultValue, string fileName)
        {
            return Load<T>(key, defaultValue, new SaveSettings(fileName));
        }

        /// <summary>
        /// Load data with the specified key using custom settings, returning the defaultValue if not found
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="defaultValue">The value to return if the key doesn't exist</param>
        /// <param name="settings">Custom save settings (if null, default settings with "save.json" will be used)</param>
        /// <returns>The loaded data or defaultValue if not found</returns>
        public static T Load<T>(string key, T defaultValue, SaveSettings settings = null)
        {
            T result = Load<T>(key, settings);
            return EqualityComparer<T>.Default.Equals(result, default) ? defaultValue : result;
        }
        #endregion

        #region Key Management
        /// <summary>
        /// Check if a key exists in the save file
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <returns>True if the key exists</returns>
        public static bool KeyExists(string key)
        {
            return KeyExists(key, (SaveSettings)null);
        }

        /// <summary>
        /// Check if a key exists in the specified file
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>True if the key exists</returns>
        public static bool KeyExists(string key, string fileName)
        {
            return KeyExists(key, new SaveSettings(fileName));
        }

        /// <summary>
        /// Check if a key exists using custom settings
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="settings">Custom save settings (if null, default settings with "save.json" will be used)</param>
        /// <returns>True if the key exists</returns>
        public static bool KeyExists(string key, SaveSettings settings = null)
        {
           

            return EzSaveCore.KeyExists(key, settings);
        }

        /// <summary>
        /// Delete a key from the save file
        /// </summary>
        /// <param name="key">Unique key to identify the data to delete</param>
        /// <returns>True if deletion was successful</returns>
        public static bool DeleteKey(string key)
        {
            return DeleteKey(key, (SaveSettings)null);
        }

        /// <summary>
        /// Delete a key from the specified file
        /// </summary>
        /// <param name="key">Unique key to identify the data to delete</param>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>True if deletion was successful</returns>
        public static bool DeleteKey(string key, string fileName)
        {
            return DeleteKey(key, new SaveSettings(fileName));
        }

        /// <summary>
        /// Delete a key using custom settings
        /// </summary>
        /// <param name="key">Unique key to identify the data to delete</param>
        /// <param name="settings">Custom save settings (if null, default settings with "save.json" will be used)</param>
        /// <returns>True if deletion was successful</returns>
        public static bool DeleteKey(string key, SaveSettings settings = null)
        {
           

            return EzSaveCore.DeleteKey(key, settings);
        }

        /// <summary>
        /// Get all keys in a save file
        /// </summary>
        /// <param name="settings">Custom save settings (if null, default settings will be used)</param>
        /// <returns>Array of keys or empty array if file doesn't exist</returns>
        public static string[] GetKeys(SaveSettings settings = null)
        {
           
            return EzSaveCore.GetKeys(settings);
        }
        #endregion

        #region Data Management
        /// <summary>
        /// Delete all saved data
        /// </summary>
        /// <returns>True if all data was successfully deleted</returns>
        public static bool DeleteAllData()
        {
            return EzSaveCore.DeleteAllData();
        }

        /// <summary>
        /// Delete all saved data with confirmation
        /// </summary>
        /// <param name="confirmationKey">A string that must match "DELETE" to confirm the operation</param>
        /// <returns>True if all data was successfully deleted</returns>
        public static bool DeleteAllData(string confirmationKey)
        {
            if (confirmationKey != "DELETE")
            {
                Debug.LogWarning("EzSave: DeleteAllData confirmation key must be 'DELETE'");
                return false;
            }

            return DeleteAllData();
        }
        #endregion

        #region File Management
        /// <summary>
        /// Check if a save file exists
        /// </summary>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>True if the file exists</returns>
        public static bool FileExists(string fileName)
        {
            var settings = new SaveSettings(fileName);
            return EzSaveCore.FileExists(settings);
        }

        /// <summary>
        /// Check if a save file exists using settings
        /// </summary>
        /// <param name="settings">Save settings</param>
        /// <returns>True if the file exists</returns>
        public static bool FileExists(SaveSettings settings)
        {
            return EzSaveCore.FileExists(settings);
        }

        /// <summary>
        /// Delete a save file
        /// </summary>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>True if deletion was successful</returns>
        public static bool DeleteFile(string fileName)
        {
            var settings = new SaveSettings(fileName);
            return EzSaveCore.DeleteFile(settings);
        }

        /// <summary>
        /// Delete a save file using settings
        /// </summary>
        /// <param name="settings">Save settings</param>
        /// <returns>True if deletion was successful</returns>
        public static bool DeleteFile(SaveSettings settings)
        {
           
            return EzSaveCore.DeleteFile(settings);
        }
        #endregion

        #region Async Save Methods
        /// <summary>
        /// Save data asynchronously with the specified key using default settings
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="data">The data to save</param>
        /// <returns>Save operation with Task and OnComplete event</returns>
        public static SaveOperation SaveAsync<T>(string key, T data)
        {
            return SaveAsync(key, data, (SaveSettings)null);
        }

        /// <summary>
        /// Save data asynchronously with the specified key to the specified file
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="data">The data to save</param>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>Save operation with Task and OnComplete event</returns>
        public static SaveOperation SaveAsync<T>(string key, T data, string fileName)
        {
            return SaveAsync(key, data, new SaveSettings(fileName));
        }

        /// <summary>
        /// Save data asynchronously with the specified key using custom settings
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="data">The data to save</param>
        /// <param name="settings">Custom save settings (if null, default settings with "save.json" will be used)</param>
        /// <returns>Save operation with Task and OnComplete event</returns>
        public static SaveOperation SaveAsync<T>(string key, T data, SaveSettings settings = null)
        {
            var task = EzSaveCore.SaveAsync(key, data, settings);
            return new SaveOperation(task);
        }
        #endregion

        #region Async Load Methods
        /// <summary>
        /// Load data asynchronously with the specified key using default settings
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <returns>Load operation with Task and OnComplete event</returns>
        public static LoadOperation<T> LoadAsync<T>(string key)
        {
            return LoadAsync<T>(key, (SaveSettings)null);
        }

        /// <summary>
        /// Load data asynchronously with the specified key from the specified file
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>Load operation with Task and OnComplete event</returns>
        public static LoadOperation<T> LoadAsync<T>(string key, string fileName)
        {
            return LoadAsync<T>(key, new SaveSettings(fileName));
        }

        /// <summary>
        /// Load data asynchronously with the specified key using custom settings
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="settings">Custom save settings (if null, default settings with "save.json" will be used)</param>
        /// <returns>Load operation with Task and OnComplete event</returns>
        public static LoadOperation<T> LoadAsync<T>(string key, SaveSettings settings = null)
        {
            var task = EzSaveCore.LoadAsync<T>(key, settings);
            return new LoadOperation<T>(task);
        }

        /// <summary>
        /// Load data asynchronously with the specified key using default settings, returning the defaultValue if not found
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="defaultValue">The value to return if the key doesn't exist</param>
        /// <returns>Load operation with Task and OnComplete event</returns>
        public static LoadOperation<T> LoadAsync<T>(string key, T defaultValue)
        {
            return LoadAsync<T>(key, defaultValue, (SaveSettings)null);
        }

        /// <summary>
        /// Load data asynchronously with the specified key from the specified file, returning the defaultValue if not found
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="defaultValue">The value to return if the key doesn't exist</param>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>Load operation with Task and OnComplete event</returns>
        public static LoadOperation<T> LoadAsync<T>(string key, T defaultValue, string fileName)
        {
            return LoadAsync<T>(key, defaultValue, new SaveSettings(fileName));
        }

        /// <summary>
        /// Load data asynchronously with the specified key using custom settings, returning the defaultValue if not found
        /// </summary>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="defaultValue">The value to return if the key doesn't exist</param>
        /// <param name="settings">Custom save settings (if null, default settings with "save.json" will be used)</param>
        /// <returns>Load operation with Task and OnComplete event</returns>
        public static LoadOperation<T> LoadAsync<T>(string key, T defaultValue, SaveSettings settings = null)
        {
            var operation = LoadAsync<T>(key, settings);
            
            // Create a new task that wraps the original task and applies the default value if needed
            var wrappedTask = operation.Task.ContinueWith(t => {
                return EqualityComparer<T>.Default.Equals(t.Result, default) ? defaultValue : t.Result;
            });
            
            return new LoadOperation<T>(wrappedTask);
        }
        #endregion

        #region Async Key Management
        /// <summary>
        /// Delete a key asynchronously from the save file
        /// </summary>
        /// <param name="key">Unique key to identify the data to delete</param>
        /// <returns>Save operation with Task and OnComplete event</returns>
        public static SaveOperation DeleteKeyAsync(string key)
        {
            return DeleteKeyAsync(key, (SaveSettings)null);
        }

        /// <summary>
        /// Delete a key asynchronously from the specified file
        /// </summary>
        /// <param name="key">Unique key to identify the data to delete</param>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>Save operation with Task and OnComplete event</returns>
        public static SaveOperation DeleteKeyAsync(string key, string fileName)
        {
            return DeleteKeyAsync(key, new SaveSettings(fileName));
        }

        /// <summary>
        /// Delete a key asynchronously using custom settings
        /// </summary>
        /// <param name="key">Unique key to identify the data to delete</param>
        /// <param name="settings">Custom save settings (if null, default settings with "save.json" will be used)</param>
        /// <returns>Save operation with Task and OnComplete event</returns>
        public static SaveOperation DeleteKeyAsync(string key, SaveSettings settings = null)
        {
            var task = EzSaveCore.DeleteKeyAsync(key, settings);
            return new SaveOperation(task);
        }
        #endregion

        #region Async File Management
        /// <summary>
        /// Delete a save file asynchronously
        /// </summary>
        /// <param name="fileName">Name of the save file</param>
        /// <returns>Save operation with Task and OnComplete event</returns>
        public static SaveOperation DeleteFileAsync(string fileName)
        {
            var settings = new SaveSettings(fileName);
            return DeleteFileAsync(settings);
        }

        /// <summary>
        /// Delete a save file asynchronously using settings
        /// </summary>
        /// <param name="settings">Save settings</param>
        /// <returns>Save operation with Task and OnComplete event</returns>
        public static SaveOperation DeleteFileAsync(SaveSettings settings)
        {
            var task = EzSaveCore.DeleteFileAsync(settings);
            return new SaveOperation(task);
        }
        #endregion
    }
} 