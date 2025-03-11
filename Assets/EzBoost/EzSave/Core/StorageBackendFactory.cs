using System;
using EzBoost.EzSave.Core;
namespace EzBoost.EzSave.Storage
{
    /// <summary>
    /// Factory class for creating storage backends based on the storage type
    /// </summary>
    public static class StorageBackendFactory
    {
        private static readonly FileSystemBackend _fileSystemBackend = new FileSystemBackend();
        private static readonly PlayerPrefsBackend _playerPrefsBackend = new PlayerPrefsBackend();
        

        /// <summary>
        /// Get a storage backend based on the specified type
        /// </summary>
        public static IStorageBackend GetBackend(StorageType storageType)
        {
            switch (storageType)
            {
                case StorageType.FileSystem:
                    return _fileSystemBackend;
                case StorageType.PlayerPrefs:
                    return _playerPrefsBackend;
                default:
                    throw new NotImplementedException($"Storage type {storageType} is not implemented");
            }
        }
    }
} 