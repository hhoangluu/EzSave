using System;
using System.IO;
using System.Threading.Tasks;
using EzBoost.EzSave.Core;
using UnityEngine;
namespace EzBoost.EzSave.Storage
{
    public class PlayerPrefsBackend : IStorageBackend
    {
        private readonly string _prefix;

        public PlayerPrefsBackend(string prefix = "EzSave_")
        {
            _prefix = prefix;
        }

        public string GetBasePath() => _prefix;

        public bool FileExists(string filePath)
        {
            string key = GetKeyFromPath(filePath);
            return PlayerPrefs.HasKey(key);
        }

        public string ReadFile(string filePath)
        {
            try
            {
                string key = GetKeyFromPath(filePath);
                if (PlayerPrefs.HasKey(key))
                {
                    return PlayerPrefs.GetString(key);
                }
                return string.Empty;
            }
            catch (Exception e)
            {
                Debug.LogError($"PlayerPrefsBackend: Failed to read key {filePath}. Error: {e.Message}");
                return string.Empty;
            }
        }

        public void WriteFile(string filePath, string contents)
        {
            try
            {
                string key = GetKeyFromPath(filePath);
                PlayerPrefs.SetString(key, contents);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"PlayerPrefsBackend: Failed to write key {filePath}. Error: {e.Message}");
            }
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                string key = GetKeyFromPath(filePath);
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                    PlayerPrefs.Save();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"PlayerPrefsBackend: Failed to delete key {filePath}. Error: {e.Message}");
                return false;
            }
        }

        public bool DeleteAllFiles()
        {
            try
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"PlayerPrefsBackend: Failed to delete all keys. Error: {e.Message}");
                return false;
            }
        }

        public Task<string> ReadFileAsync(string filePath)
        {
            return Task.FromResult(ReadFile(filePath));
        }

        public Task WriteFileAsync(string filePath, string contents)
        {
            WriteFile(filePath, contents);
            return Task.CompletedTask;
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            return Task.FromResult(DeleteFile(filePath));
        }

        public Task<bool> DeleteAllFilesAsync()
        {
            return Task.FromResult(DeleteAllFiles());
        }

        private string GetKeyFromPath(string filePath)
        {
            // Convert file path to a valid PlayerPrefs key
            string fileName = Path.GetFileName(filePath);
            string directory = Path.GetDirectoryName(filePath)?.Replace('\\', '_').Replace('/', '_') ?? "";

            if (string.IsNullOrEmpty(directory))
                return _prefix + fileName;
            else
                return _prefix + directory + "_" + fileName;
        }
    }
}