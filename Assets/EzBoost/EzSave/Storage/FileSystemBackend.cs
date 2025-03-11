using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EzBoost.EzSave.Core;
using UnityEngine;
namespace EzBoost.EzSave.Storage
{
    public class FileSystemBackend : IStorageBackend
    {
        private readonly string _basePath;

        public FileSystemBackend(string basePath = null)
        {
            _basePath = basePath ?? Application.persistentDataPath + "/EzSave/";
            EnsureDirectoryExists(_basePath);
        }

        public string GetBasePath() => _basePath;

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public string ReadFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"FileSystemBackend: Failed to read file {filePath}. Error: {e.Message}");
                return string.Empty;
            }
        }

        public void WriteFile(string filePath, string contents)
        {
            try
            {
                EnsureDirectoryExists(Path.GetDirectoryName(filePath));
                File.WriteAllText(filePath, contents);
            }
            catch (Exception e)
            {
                Debug.LogError($"FileSystemBackend: Failed to write file {filePath}. Error: {e.Message}");
            }
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"FileSystemBackend: Failed to delete file {filePath}. Error: {e.Message}");
                return false;
            }
        }

        public bool DeleteAllFiles()
        {
            try
            {
                if (Directory.Exists(_basePath))
                {
                    Directory.Delete(_basePath, true);
                    Directory.CreateDirectory(_basePath);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"FileSystemBackend: Failed to delete all files. Error: {e.Message}");
                return false;
            }
        }

        public async Task<string> ReadFileAsync(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"FileSystemBackend: Failed to read file {filePath} asynchronously. Error: {e.Message}");
                return string.Empty;
            }
        }

        public async Task WriteFileAsync(string filePath, string contents)
        {
            try
            {
                EnsureDirectoryExists(Path.GetDirectoryName(filePath));
                using (var writer = new StreamWriter(filePath))
                {
                    await writer.WriteAsync(contents);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"FileSystemBackend: Failed to write file {filePath} asynchronously. Error: {e.Message}");
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            return await Task.Run(() => DeleteFile(filePath));
        }

        public async Task<bool> DeleteAllFilesAsync()
        {
            return await Task.Run(() => DeleteAllFiles());
        }

        private void EnsureDirectoryExists(string directoryPath)
        {
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
    }
}