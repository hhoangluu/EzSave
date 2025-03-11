using System;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using EzBoost.EzSave.Storage;
using EzBoost.EzSave.Crypto;

namespace EzBoost.EzSave.Core
{

    internal static class EzSaveIO
    {
        private static readonly string _savePath = Application.persistentDataPath + "/EzSave/";

        /// <summary>
        /// Initialize the save directory
        /// </summary>
        internal static void Initialize()
        {
            Debug.Log("EzSaveIO: Ensuring save directory exists at: " + _savePath);
            EnsureDirectoryExists(_savePath);
            Debug.Log("EzSaveIO: Save directory ready");
        }

        /// <summary>
        /// Get a storage backend based on the specified type
        /// </summary>
        internal static IStorageBackend GetBackend(StorageType storageType)
        {
            return StorageBackendFactory.GetBackend(storageType);
        }

        /// <summary>
        /// Get the full file path for a file name
        /// </summary>
        internal static string GetFilePath(string fileName, SaveSettings settings = null)
        {
            if (settings == null)
            {
                return Path.Combine(_savePath, fileName);
            }

            string basePath = GetBasePath(settings.StorageType);
            string filePath = settings.GetFilePath(basePath);
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(filePath);
            EnsureDirectoryExists(directory);
            
            return filePath;
        }

        /// <summary>
        /// Converts a standard file path to its encrypted equivalent by adding the encryption suffix
        /// </summary>
        internal static string GetEncryptedFilePath(string filePath, SaveSettings settings)
        {
            if (settings.EncryptionType == Crypto.EncryptionType.None)
            {
                return filePath; // No change if encryption is disabled
            }
            
            // Check if the file already has the encryption suffix
            if (!filePath.EndsWith(SaveSettings.ENCRYPTED_FILE_SUFFIX))
            {
                return filePath + SaveSettings.ENCRYPTED_FILE_SUFFIX;
            }
            
            return filePath;
        }

        /// <summary>
        /// Check if a file exists in encrypted or unencrypted form
        /// </summary>
        internal static bool FileExists(string filePath, SaveSettings settings)
        {
            var backend = GetBackend(settings.StorageType);
            
            // First try with the path as provided (might already have suffix)
            if (backend.FileExists(filePath))
            {
                return true;
            }
            
            // If encryption is enabled, also check for the encrypted version
            if (settings.EncryptionType != Crypto.EncryptionType.None)
            {
                string encryptedPath = GetEncryptedFilePath(filePath, settings);
                if (encryptedPath != filePath && backend.FileExists(encryptedPath))
                {
                    return true;
                }
            }
            // If encryption is disabled, also check for an unencrypted version
            else if (filePath.EndsWith(SaveSettings.ENCRYPTED_FILE_SUFFIX))
            {
                string unencryptedPath = filePath.Substring(0, filePath.Length - SaveSettings.ENCRYPTED_FILE_SUFFIX.Length);
                if (backend.FileExists(unencryptedPath))
                {
                    return true;
                }
            }
            
            return false;
        }

        private static string GetBasePath(StorageType storageType)
        {
            return GetBackend(storageType).GetBasePath();
        }

        /// <summary>
        /// Ensure a directory exists, creating it if necessary
        /// </summary>
        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Read the contents of a file
        /// </summary>
        internal static string ReadFile(string filePath, SaveSettings settings)
        {
            var backend = GetBackend(settings.StorageType);
            
            // First try with the path as provided
            if (backend.FileExists(filePath))
            {
                return backend.ReadFile(filePath);
            }
            
            // If encryption is enabled, check for the encrypted version
            if (settings.EncryptionType != Crypto.EncryptionType.None)
            {
                string encryptedPath = GetEncryptedFilePath(filePath, settings);
                if (encryptedPath != filePath && backend.FileExists(encryptedPath))
                {
                    return backend.ReadFile(encryptedPath);
                }
            }
            // If encryption is disabled, check for an unencrypted version
            else if (filePath.EndsWith(SaveSettings.ENCRYPTED_FILE_SUFFIX))
            {
                string unencryptedPath = filePath.Substring(0, filePath.Length - SaveSettings.ENCRYPTED_FILE_SUFFIX.Length);
                if (backend.FileExists(unencryptedPath))
                {
                    return backend.ReadFile(unencryptedPath);
                }
            }
            
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        /// <summary>
        /// Write contents to a file
        /// </summary>
        internal static void WriteFile(string filePath, string contents, SaveSettings settings)
        {
            try
            {
                // If encryption is enabled, append the suffix
                if (settings.EncryptionType != Crypto.EncryptionType.None)
                {
                    filePath = GetEncryptedFilePath(filePath, settings);
                }
                // If encryption is disabled but path has encryption suffix, remove it
                else if (filePath.EndsWith(SaveSettings.ENCRYPTED_FILE_SUFFIX))
                {
                    filePath = filePath.Substring(0, filePath.Length - SaveSettings.ENCRYPTED_FILE_SUFFIX.Length);
                }
                
                // Ensure directory exists
                string directory = Path.GetDirectoryName(filePath);
                EnsureDirectoryExists(directory);
                
                // Write to file
                GetBackend(settings.StorageType).WriteFile(filePath, contents);
            }
            catch (Exception ex)
            {
                Debug.LogError($"EzSaveIO: Error writing file {filePath}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete a file
        /// </summary>
        internal static bool DeleteFile(string filePath, SaveSettings settings)
        {
            var backend = GetBackend(settings.StorageType);
            bool deleted = false;
            
            // Try to delete the file as provided
            if (backend.FileExists(filePath))
            {
                deleted = backend.DeleteFile(filePath);
            }
            
            // If encryption is enabled, also try to delete the encrypted version
            if (settings.EncryptionType != Crypto.EncryptionType.None)
            {
                string encryptedPath = GetEncryptedFilePath(filePath, settings);
                if (encryptedPath != filePath && backend.FileExists(encryptedPath))
                {
                    deleted = backend.DeleteFile(encryptedPath) || deleted;
                }
            }
            // If encryption is disabled, also try to delete the unencrypted version
            else if (filePath.EndsWith(SaveSettings.ENCRYPTED_FILE_SUFFIX))
            {
                string unencryptedPath = filePath.Substring(0, filePath.Length - SaveSettings.ENCRYPTED_FILE_SUFFIX.Length);
                if (backend.FileExists(unencryptedPath))
                {
                    deleted = backend.DeleteFile(unencryptedPath) || deleted;
                }
            }
            
            return deleted;
        }

        /// <summary>
        /// Get the save path for logging purposes
        /// </summary>
        internal static string GetSavePath()
        {
            return _savePath;
        }

        /// <summary>
        /// Delete all files in the save directory
        /// </summary>
        internal static bool DeleteAllFiles(SaveSettings settings)
        {
            return GetBackend(settings.StorageType).DeleteAllFiles();
        }

        /// <summary>
        /// Asynchronously read the contents of a file
        /// </summary>
        internal static async Task<string> ReadFileAsync(string filePath, SaveSettings settings)
        {
            var backend = GetBackend(settings.StorageType);
            
            // First try with the path as provided
            if (backend.FileExists(filePath))
            {
                return await backend.ReadFileAsync(filePath);
            }
            
            // If encryption is enabled, check for the encrypted version
            if (settings.EncryptionType != Crypto.EncryptionType.None)
            {
                string encryptedPath = GetEncryptedFilePath(filePath, settings);
                if (encryptedPath != filePath && backend.FileExists(encryptedPath))
                {
                    return await backend.ReadFileAsync(encryptedPath);
                }
            }
            // If encryption is disabled, check for an unencrypted version
            else if (filePath.EndsWith(SaveSettings.ENCRYPTED_FILE_SUFFIX))
            {
                string unencryptedPath = filePath.Substring(0, filePath.Length - SaveSettings.ENCRYPTED_FILE_SUFFIX.Length);
                if (backend.FileExists(unencryptedPath))
                {
                    return await backend.ReadFileAsync(unencryptedPath);
                }
            }
            
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        /// <summary>
        /// Asynchronously write contents to a file
        /// </summary>
        internal static async Task WriteFileAsync(string filePath, string contents, SaveSettings settings)
        {
            try
            {
                // If encryption is enabled, append the suffix
                if (settings.EncryptionType != Crypto.EncryptionType.None)
                {
                    filePath = GetEncryptedFilePath(filePath, settings);
                }
                // If encryption is disabled but path has encryption suffix, remove it
                else if (filePath.EndsWith(SaveSettings.ENCRYPTED_FILE_SUFFIX))
                {
                    filePath = filePath.Substring(0, filePath.Length - SaveSettings.ENCRYPTED_FILE_SUFFIX.Length);
                }
                
                // Ensure directory exists
                string directory = Path.GetDirectoryName(filePath);
                EnsureDirectoryExists(directory);
                
                // Write to file
                await GetBackend(settings.StorageType).WriteFileAsync(filePath, contents);
            }
            catch (Exception ex)
            {
                Debug.LogError($"EzSaveIO: Error writing file {filePath}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously delete a file
        /// </summary>
        internal static async Task<bool> DeleteFileAsync(string filePath, SaveSettings settings)
        {
            var backend = GetBackend(settings.StorageType);
            bool deleted = false;
            
            // Try to delete the file as provided
            if (backend.FileExists(filePath))
            {
                deleted = await backend.DeleteFileAsync(filePath);
            }
            
            // If encryption is enabled, also try to delete the encrypted version
            if (settings.EncryptionType != Crypto.EncryptionType.None)
            {
                string encryptedPath = GetEncryptedFilePath(filePath, settings);
                if (encryptedPath != filePath && backend.FileExists(encryptedPath))
                {
                    bool encryptedDeleted = await backend.DeleteFileAsync(encryptedPath);
                    deleted = encryptedDeleted || deleted;
                }
            }
            // If encryption is disabled, also try to delete the unencrypted version
            else if (filePath.EndsWith(SaveSettings.ENCRYPTED_FILE_SUFFIX))
            {
                string unencryptedPath = filePath.Substring(0, filePath.Length - SaveSettings.ENCRYPTED_FILE_SUFFIX.Length);
                if (backend.FileExists(unencryptedPath))
                {
                    bool unencryptedDeleted = await backend.DeleteFileAsync(unencryptedPath);
                    deleted = unencryptedDeleted || deleted;
                }
            }
            
            return deleted;
        }

        /// <summary>
        /// Delete all files in the save directory asynchronously
        /// </summary>
        internal static async Task<bool> DeleteAllFilesAsync(SaveSettings settings)
        {
            return await GetBackend(settings.StorageType).DeleteAllFilesAsync();
        }

        #region Legacy Methods (For Backward Compatibility)

        /// <summary>
        /// Check if a file exists using the specified storage type
        /// </summary>
        internal static bool FileExists(string filePath, StorageType storageType = StorageType.FileSystem)
        {
            return GetBackend(storageType).FileExists(filePath);
        }

        /// <summary>
        /// Read the contents of a file using the specified storage type
        /// </summary>
        internal static string ReadFile(string filePath, StorageType storageType = StorageType.FileSystem)
        {
            var settings = new SaveSettings(Path.GetFileName(filePath)) 
            { 
                StorageType = storageType
            };
            return ReadFile(filePath, settings);
        }

        /// <summary>
        /// Write contents to a file using the specified storage type
        /// </summary>
        internal static void WriteFile(string filePath, string contents, StorageType storageType = StorageType.FileSystem)
        {
            var settings = new SaveSettings(Path.GetFileName(filePath)) 
            { 
                StorageType = storageType
            };
            WriteFile(filePath, contents, settings);
        }

        /// <summary>
        /// Delete a file using the specified storage type
        /// </summary>
        internal static bool DeleteFile(string filePath, StorageType storageType = StorageType.FileSystem)
        {
            return GetBackend(storageType).DeleteFile(filePath);
        }

        /// <summary>
        /// Delete all files using the specified storage type
        /// </summary>
        internal static bool DeleteAllFiles(StorageType storageType = StorageType.FileSystem)
        {
            return GetBackend(storageType).DeleteAllFiles();
        }

        /// <summary>
        /// Read the contents of a file asynchronously using the specified storage type
        /// </summary>
        internal static async Task<string> ReadFileAsync(string filePath, StorageType storageType = StorageType.FileSystem)
        {
            var settings = new SaveSettings(Path.GetFileName(filePath)) 
            { 
                StorageType = storageType
            };
            return await ReadFileAsync(filePath, settings);
        }

        /// <summary>
        /// Write contents to a file asynchronously using the specified storage type
        /// </summary>
        internal static async Task WriteFileAsync(string filePath, string contents, StorageType storageType = StorageType.FileSystem)
        {
            var settings = new SaveSettings(Path.GetFileName(filePath)) 
            { 
                StorageType = storageType
            };
            await WriteFileAsync(filePath, contents, settings);
        }

        /// <summary>
        /// Delete a file asynchronously using the specified storage type
        /// </summary>
        internal static async Task<bool> DeleteFileAsync(string filePath, StorageType storageType = StorageType.FileSystem)
        {
            return await GetBackend(storageType).DeleteFileAsync(filePath);
        }

        /// <summary>
        /// Delete all files asynchronously using the specified storage type
        /// </summary>
        internal static async Task<bool> DeleteAllFilesAsync(StorageType storageType = StorageType.FileSystem)
        {
            return await GetBackend(storageType).DeleteAllFilesAsync();
        }

        #endregion
    }
} 