using System;
using UnityEngine;
using EzBoost.EzSave.Core;

namespace EzBoost.EzSave
{
    /// <summary>
    /// Settings class for configuring save/load operations
    /// </summary>
    [Serializable]
    public class SaveSettings
    {
        /// <summary>
        /// Suffix to add to encrypted files to distinguish them from unencrypted files
        /// </summary>
        public const string ENCRYPTED_FILE_SUFFIX = ".encrypted";

        /// <summary>
        /// Name of the save file
        /// </summary>
        [SerializeField] private string _fileName = "game_data.json";
        public string FileName
        {
            get => _fileName;
            set => _fileName = value;
        }

        /// <summary>
        /// Optional subfolder within the main save directory
        /// </summary>
        [SerializeField] private string _subFolder;
        public string SubFolder
        {
            get => _subFolder;
            set => _subFolder = value;
        }

        /// <summary>
        /// Whether to compress the saved data
        /// </summary>
        [SerializeField] private bool _useCompression;
        public bool UseCompression
        {
            get => _useCompression;
            set => _useCompression = value;
        }

        /// <summary>
        /// The encryption type to use for saving data
        /// </summary>
        [SerializeField]
        private Crypto.EncryptionType _encryptionType = Crypto.EncryptionType.None;
        
        /// <summary>
        /// Property to get/set the encryption type
        /// </summary>
        public Crypto.EncryptionType EncryptionType
        {
            get => _encryptionType;
            set => _encryptionType = value;
        }

        /// <summary>
        /// Password used for encryption/decryption (not serialized for security)
        /// </summary>
        [SerializeField]
        private string _password = "password";
        
        /// <summary>
        /// Property to get/set the encryption password
        /// </summary>
        public string Password
        {
            get => _password;
            set => _password = value;
        }
        
        // For serialization purposes
        [SerializeField]
        private StorageType _storageType = StorageType.FileSystem;
        
        // Property to get/set the storage type
        public StorageType StorageType
        {
            get => _storageType;
            set => _storageType = value;
        }

        /// <summary>
        /// Default constructor with required filename
        /// </summary>
        /// <param name="fileName">Name of the save file</param>
        public SaveSettings(string fileName)
        {
            FileName = fileName;
            SubFolder = string.Empty;
            UseCompression = false;
            _encryptionType = Crypto.EncryptionType.None;
            _password = string.Empty;
            _storageType = StorageType.FileSystem;
        }

        /// <summary>
        /// Constructor with all options
        /// </summary>
        public SaveSettings(string fileName, string subFolder, bool useCompression, Crypto.EncryptionType encryptionType)
        {
            FileName = fileName;
            SubFolder = subFolder;
            UseCompression = useCompression;
            _encryptionType = encryptionType;
            _password = string.Empty;
            _storageType = StorageType.FileSystem;
        }
        
        /// <summary>
        /// Constructor with all options including storage type
        /// </summary>
        public SaveSettings(string fileName, string subFolder, bool useCompression, Crypto.EncryptionType encryptionType, StorageType storageType)
        {
            FileName = fileName;
            SubFolder = subFolder;
            UseCompression = useCompression;
            _encryptionType = encryptionType;
            _password = string.Empty;
            _storageType = storageType;
        }
        
        /// <summary>
        /// Constructor with all options including password
        /// </summary>
        public SaveSettings(string fileName, string subFolder, bool useCompression, Crypto.EncryptionType encryptionType, string password)
        {
            FileName = fileName;
            SubFolder = subFolder;
            UseCompression = useCompression;
            _encryptionType = encryptionType;
            _password = password;
            _storageType = StorageType.FileSystem;
        }
        
        /// <summary>
        /// Constructor with all options including password and storage type
        /// </summary>
        public SaveSettings(string fileName, string subFolder, bool useCompression, Crypto.EncryptionType encryptionType, string password, StorageType storageType)
        {
            FileName = fileName;
            SubFolder = subFolder;
            UseCompression = useCompression;
            _encryptionType = encryptionType;
            _password = password;
            _storageType = storageType;
        }

        /// <summary>
        /// Generate the full file path based on the settings
        /// </summary>
        public string GetFilePath(string basePath)
        {
            string path = basePath;
            
            // Add subfolder if specified
            if (!string.IsNullOrEmpty(_subFolder))
            {
                path = System.IO.Path.Combine(path, _subFolder);
            }
            
            // Add file name
            string fileNameWithSuffix = _fileName;
            
            // Add encryption suffix if encryption is enabled
            if (_encryptionType != Crypto.EncryptionType.None)
            {
                // Check if file already has the encrypted suffix
                if (!fileNameWithSuffix.EndsWith(ENCRYPTED_FILE_SUFFIX))
                {
                    fileNameWithSuffix += ENCRYPTED_FILE_SUFFIX;
                }
            }
            
            return System.IO.Path.Combine(path, fileNameWithSuffix);
        }
    }
} 