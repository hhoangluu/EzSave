using System;

namespace EzBoost.EzSave.Crypto
{
    /// <summary>
    /// Defines the available encryption types for EzSave
    /// </summary>
    [Serializable]
    public enum EncryptionType
    {
        /// <summary>
        /// No encryption, data is stored as plain text
        /// </summary>
        None,
        
        /// <summary>
        /// AES encryption, provides strong security for mobile games
        /// </summary>
        AES
    }
} 