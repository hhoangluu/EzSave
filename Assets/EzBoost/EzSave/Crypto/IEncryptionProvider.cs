using System;

namespace EzBoost.EzSave.Crypto
{
        public interface IEncryptionProvider
    {
                /// <param name="data">The data to encrypt</param>
        /// <returns>The encrypted data</returns>
        string Encrypt(string data);
        
                /// <param name="encryptedData">The encrypted data</param>
        /// <returns>The decrypted data</returns>
        string Decrypt(string encryptedData);
        
                /// <param name="data">The data to encrypt</param>
        /// <param name="password">The password to use for encryption</param>
        /// <returns>The encrypted data</returns>
        string Encrypt(string data, string password);
        
                /// <param name="encryptedData">The encrypted data</param>
        /// <param name="password">The password to use for decryption</param>
        /// <returns>The decrypted data</returns>
        string Decrypt(string encryptedData, string password);
        
                EncryptionType EncryptionType { get; }
    }
} 