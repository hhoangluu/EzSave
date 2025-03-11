using System;

namespace EzBoost.EzSave.Crypto
{
    /// <summary>
    /// Interface for encryption providers in EzSave
    /// </summary>
    public interface IEncryptionProvider
    {
        /// <summary>
        /// Encrypts a string and returns the encrypted data
        /// </summary>
        /// <param name="data">The data to encrypt</param>
        /// <returns>The encrypted data</returns>
        string Encrypt(string data);
        
        /// <summary>
        /// Decrypts an encrypted string
        /// </summary>
        /// <param name="encryptedData">The encrypted data</param>
        /// <returns>The decrypted data</returns>
        string Decrypt(string encryptedData);
        
        /// <summary>
        /// Encrypts a string using a password and returns the encrypted data
        /// </summary>
        /// <param name="data">The data to encrypt</param>
        /// <param name="password">The password to use for encryption</param>
        /// <returns>The encrypted data</returns>
        string Encrypt(string data, string password);
        
        /// <summary>
        /// Decrypts an encrypted string using a password
        /// </summary>
        /// <param name="encryptedData">The encrypted data</param>
        /// <param name="password">The password to use for decryption</param>
        /// <returns>The decrypted data</returns>
        string Decrypt(string encryptedData, string password);
        
        /// <summary>
        /// Gets the encryption type provided by this provider
        /// </summary>
        EncryptionType EncryptionType { get; }
    }
} 