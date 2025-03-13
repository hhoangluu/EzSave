using System;
using UnityEngine;
using EzBoost.EzSave.Core;

namespace EzBoost.EzSave
{
    [Serializable]
    public class SaveSettings
    {
        public const string ENCRYPTED_FILE_SUFFIX = ".encrypted";

        [SerializeField] private string _fileName = "game_data.json";
        public string FileName
        {
            get => _fileName;
            set => _fileName = value;
        }

        [SerializeField] private string _subFolder;
        public string SubFolder
        {
            get => _subFolder;
            set => _subFolder = value;
        }

        [SerializeField] private bool _useCompression;
        public bool UseCompression
        {
            get => _useCompression;
            set => _useCompression = value;
        }

        [SerializeField]
        private Crypto.EncryptionType _encryptionType = Crypto.EncryptionType.None;

        public Crypto.EncryptionType EncryptionType
        {
            get => _encryptionType;
            set => _encryptionType = value;
        }

        [SerializeField]
        private string _password = "password";

        public string Password
        {
            get => _password;
            set => _password = value;
        }

        [SerializeField]
        private StorageType _storageType = StorageType.FileSystem;

        public StorageType StorageType
        {
            get => _storageType;
            set => _storageType = value;
        }

        public SaveSettings(string fileName)
        {
            FileName = fileName;
            SubFolder = string.Empty;
            UseCompression = false;
            _encryptionType = Crypto.EncryptionType.None;
            _password = string.Empty;
            _storageType = StorageType.FileSystem;
        }

        public SaveSettings(string fileName, string subFolder, bool useCompression, Crypto.EncryptionType encryptionType)
        {
            FileName = fileName;
            SubFolder = subFolder;
            UseCompression = useCompression;
            _encryptionType = encryptionType;
            _password = string.Empty;
            _storageType = StorageType.FileSystem;
        }

        public SaveSettings(string fileName, string subFolder, bool useCompression, Crypto.EncryptionType encryptionType, StorageType storageType)
        {
            FileName = fileName;
            SubFolder = subFolder;
            UseCompression = useCompression;
            _encryptionType = encryptionType;
            _password = string.Empty;
            _storageType = storageType;
        }

        public SaveSettings(string fileName, string subFolder, bool useCompression, Crypto.EncryptionType encryptionType, string password)
        {
            FileName = fileName;
            SubFolder = subFolder;
            UseCompression = useCompression;
            _encryptionType = encryptionType;
            _password = password;
            _storageType = StorageType.FileSystem;
        }

        public SaveSettings(string fileName, string subFolder, bool useCompression, Crypto.EncryptionType encryptionType, string password, StorageType storageType)
        {
            FileName = fileName;
            SubFolder = subFolder;
            UseCompression = useCompression;
            _encryptionType = encryptionType;
            _password = password;
            _storageType = storageType;
        }

        public string GetFilePath(string basePath)
        {
            string path = basePath;

            if (!string.IsNullOrEmpty(_subFolder))
            {
                path = System.IO.Path.Combine(path, _subFolder);
            }

            string fileNameWithSuffix = _fileName;

            if (_encryptionType != Crypto.EncryptionType.None)
            {
                if (!fileNameWithSuffix.EndsWith(ENCRYPTED_FILE_SUFFIX))
                {
                    fileNameWithSuffix += ENCRYPTED_FILE_SUFFIX;
                }
            }

            return System.IO.Path.Combine(path, fileNameWithSuffix);
        }
    }
}