using System;
using UnityEngine;
using EzBoost.EzSave.Crypto;

namespace EzBoost.EzSave.Core
{
        internal static class EzSaveCrypto
    {
                /// <param name="data">The data to encrypt</param>
        /// <param name="settings">Save settings containing encryption preferences</param>
        /// <returns>The encrypted data, or the original data if encryption is not enabled</returns>
        internal static string Encrypt(string data, SaveSettings settings)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            try
            {
                if (settings.EncryptionType != EncryptionType.None)
                {
                    var provider = EncryptionProviderFactory.GetProvider(settings.EncryptionType);
                    
                    // Use password if provided
                    if (!string.IsNullOrEmpty(settings.Password))
                    {
                        return provider.Encrypt(data, settings.Password);
                    }
                    
                    return provider.Encrypt(data);
                }
                
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"EzSaveCrypto: Error encrypting data: {ex.Message}");
                return data;  // Return original data on failure
            }
        }

                /// <param name="data">The data to decrypt</param>
        /// <param name="settings">Save settings containing encryption preferences</param>
        /// <returns>The decrypted data, or the original data if encryption is not enabled</returns>
        internal static string Decrypt(string data, SaveSettings settings)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            try
            {
                if (settings.EncryptionType != EncryptionType.None)
                {
                    var provider = EncryptionProviderFactory.GetProvider(settings.EncryptionType);
                    
                    // Use password if provided
                    if (!string.IsNullOrEmpty(settings.Password))
                    {
                        return provider.Decrypt(data, settings.Password);
                    }
                    
                    return provider.Decrypt(data);
                }
                
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"EzSaveCrypto: Error decrypting data: {ex.Message}");
                return data;  // Return original data on failure
            }
        }
    }
} 