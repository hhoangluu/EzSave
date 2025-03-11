using System;
using UnityEngine;

namespace EzBoost.EzSave.Crypto
{
    /// <summary>
    /// Factory class for creating encryption providers based on the encryption type
    /// </summary>
    public static class EncryptionProviderFactory
    {
        private static readonly IEncryptionProvider _aesEncryption;

        static EncryptionProviderFactory()
        {
            try
            {
                _aesEncryption = new AesEncryption();
            }
            catch (Exception ex)
            {
                Debug.LogError($"EncryptionProviderFactory: Failed to initialize encryption providers. Error: {ex.Message}");
                // Ensure we at least have NoEncryption available as fallback
            }
        }

        /// <summary>
        /// Get an encryption provider based on the specified type
        /// </summary>
        public static IEncryptionProvider GetProvider(EncryptionType encryptionType)
        {
            switch (encryptionType)
            {
                case EncryptionType.AES:
                    return _aesEncryption;
                default:
                    throw new ArgumentException($"Unsupported encryption type: {encryptionType}");
            }
        }
    }
}