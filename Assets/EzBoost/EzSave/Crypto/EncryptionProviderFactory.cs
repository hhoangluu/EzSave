using System;
using UnityEngine;

namespace EzBoost.EzSave.Crypto
{
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