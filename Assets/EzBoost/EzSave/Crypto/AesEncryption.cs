using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

namespace EzBoost.EzSave.Crypto
{
    /// <summary>
    /// Implementation of IEncryptionProvider that uses AES encryption
    /// AES is a good option for mobile games as it provides strong security with good performance
    /// </summary>
    public class AesEncryption : IEncryptionProvider
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private const int ITERATION_COUNT = 1000;
        private const int KEY_SIZE = 32; // 256 bits
        private const int IV_SIZE = 16;  // 128 bits
        
        /// <summary>
        /// Creates a new AES encryption provider with the default key and IV
        /// </summary>
        public AesEncryption() : this(GetDefaultKey(), GetDefaultIV())
        {
        }
        
        /// <summary>
        /// Creates a new AES encryption provider with the specified key and IV
        /// </summary>
        public AesEncryption(byte[] key, byte[] iv)
        {
            if (key == null || key.Length != KEY_SIZE)
                throw new ArgumentException($"Key must be {KEY_SIZE} bytes ({KEY_SIZE * 8} bits)", nameof(key));
                
            if (iv == null || iv.Length != IV_SIZE)
                throw new ArgumentException($"IV must be {IV_SIZE} bytes ({IV_SIZE * 8} bits)", nameof(iv));
                
            _key = key;
            _iv = iv;
        }

        /// <summary>
        /// Encrypts a string using AES encryption with a specific password
        /// </summary>
        public string Encrypt(string data, string password)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            if (string.IsNullOrEmpty(password))
                return Encrypt(data); // Use default key if no password
                
            try
            {
                // Generate a random salt
                byte[] salt = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(salt);
                }
                
                // Derive key and IV from password
                byte[] key = DeriveKeyFromPassword(password, salt, out byte[] iv);
                
                // Encrypt the data
                string encryptedData = EncryptWithKeyAndIV(data, key, iv);
                
                // Combine salt with encrypted data (salt + encrypted data)
                string saltBase64 = Convert.ToBase64String(salt);
                return saltBase64 + ":" + encryptedData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"AesEncryption: Error encrypting data with password: {ex.Message}");
                return data;  // Return original data on failure
            }
        }

        /// <summary>
        /// Decrypts an AES encrypted string with a specific password
        /// </summary>
        public string Decrypt(string encryptedData, string password)
        {
            if (string.IsNullOrEmpty(encryptedData))
                return encryptedData;

            if (string.IsNullOrEmpty(password))
                return Decrypt(encryptedData); // Use default key if no password

            try
            {
                // Debug info
                Debug.Log($"AesEncryption: Attempting to decrypt data (length: {encryptedData?.Length ?? 0})");
                Debug.Log($"AesEncryption: Data preview: {encryptedData?.Substring(0, Math.Min(20, encryptedData?.Length ?? 0))}...");
                
                // Check if the data contains the salt separator
                if (encryptedData.Contains(":"))
                {
                    Debug.Log("AesEncryption: Found ':' separator - using new format");
                    
                    // New format: [salt]:[encrypted data]
                    string[] parts = encryptedData.Split(new char[] { ':' }, 2);
                    if (parts.Length != 2)
                    {
                        Debug.LogError("AesEncryption: Invalid encrypted data format. Expected format: [salt]:[encrypted data]");
                        return encryptedData;
                    }
                    
                    Debug.Log($"AesEncryption: Salt part length: {parts[0]?.Length ?? 0}, Data part length: {parts[1]?.Length ?? 0}");
                    
                    try
                    {
                        // Extract salt and encrypted content
                        byte[] salt = Convert.FromBase64String(parts[0]);
                        Debug.Log($"AesEncryption: Successfully decoded salt from Base64 (length: {salt.Length})");
                        
                        string encryptedContent = parts[1];
                        Debug.Log($"AesEncryption: Encrypted content preview: {encryptedContent.Substring(0, Math.Min(20, encryptedContent.Length))}...");
                        
                        // Derive the same key and IV using the stored salt
                        byte[] key = DeriveKeyFromPassword(password, salt, out byte[] iv);
                        Debug.Log($"AesEncryption: Successfully derived key (length: {key.Length}) and IV (length: {iv.Length})");
                        
                        // Decrypt the data
                        Debug.Log("AesEncryption: Attempting to decrypt with derived key and IV");
                        return DecryptWithKeyAndIV(encryptedContent, key, iv);
                    }
                    catch (FormatException fe)
                    {
                        Debug.LogError($"AesEncryption: Base64 format error in new format: {fe.Message}");
                        Debug.LogError($"AesEncryption: Salt part that failed Base64 decode: '{parts[0]}'");
                        throw;
                    }
                }
                else
                {
                    Debug.Log("AesEncryption: No ':' separator found - using backwards compatibility mode");
                    
                    // Old format (backwards compatibility): Handle data that was encrypted before salt was stored
                    Debug.Log("AesEncryption: Using backwards compatibility mode for old data format");
                    
                    // Use a fixed salt derived from the password for backwards compatibility
                    byte[] salt = DeriveFixedSaltFromPassword(password);
                    Debug.Log($"AesEncryption: Generated fixed salt from password (length: {salt.Length})");
                    
                    byte[] key = DeriveKeyFromPassword(password, salt, out byte[] iv);
                    Debug.Log($"AesEncryption: Successfully derived key (length: {key.Length}) and IV (length: {iv.Length})");
                    
                    // Attempt to decrypt with the derived key
                    Debug.Log("AesEncryption: Attempting to decrypt with derived key and IV using backwards compatibility");
                    return DecryptWithKeyAndIV(encryptedData, key, iv);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"AesEncryption: Error decrypting data with password: {ex.Message}");
                Debug.LogError($"AesEncryption: Stack trace: {ex.StackTrace}");
                return encryptedData;  // Return encrypted data on failure
            }
        }
        
        /// <summary>
        /// Encrypts a string using AES encryption with the default key and IV
        /// </summary>
        public string Encrypt(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;
                
            try
            {
                return EncryptWithKeyAndIV(data, _key, _iv);
            }
            catch (Exception ex)
            {
                Debug.LogError($"AesEncryption: Error encrypting data: {ex.Message}");
                return data;  // Return original data on failure
            }
        }
        
        /// <summary>
        /// Decrypts an AES encrypted string with the default key and IV
        /// </summary>
        public string Decrypt(string encryptedData)
        {
            if (string.IsNullOrEmpty(encryptedData))
                return encryptedData;
                
            try
            {
                return DecryptWithKeyAndIV(encryptedData, _key, _iv);
            }
            catch (Exception ex)
            {
                Debug.LogError($"AesEncryption: Error decrypting data: {ex.Message}");
                return encryptedData;  // Return encrypted data on failure
            }
        }

        /// <summary>
        /// Helper method to encrypt data with specific key and IV
        /// </summary>
        private string EncryptWithKeyAndIV(string data, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(data);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to decrypt data with specific key and IV
        /// </summary>
        private string DecryptWithKeyAndIV(string encryptedData, byte[] key, byte[] iv)
        {
            try
            {
                Debug.Log($"DecryptWithKeyAndIV: Attempting to convert from Base64 (data length: {encryptedData.Length})");
                byte[] cipherBytes = Convert.FromBase64String(encryptedData);
                Debug.Log($"DecryptWithKeyAndIV: Successfully converted from Base64 to byte array (length: {cipherBytes.Length})");
                
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    Debug.Log("DecryptWithKeyAndIV: Created decryptor");
                    
                    using (MemoryStream ms = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                Debug.Log("DecryptWithKeyAndIV: Starting to read decrypted data");
                                string result = sr.ReadToEnd();
                                Debug.Log($"DecryptWithKeyAndIV: Successfully decrypted (result length: {result.Length})");
                                return result;
                            }
                        }
                    }
                }
            }
            catch (FormatException fe)
            {
                Debug.LogError($"DecryptWithKeyAndIV: Base64 format error: {fe.Message}");
                Debug.LogError($"DecryptWithKeyAndIV: Data that failed Base64 decode: '{encryptedData.Substring(0, Math.Min(50, encryptedData.Length))}'...");
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"DecryptWithKeyAndIV: Error: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// The encryption type is AES
        /// </summary>
        public EncryptionType EncryptionType => EncryptionType.AES;
        
        /// <summary>
        /// Derives a key from a password using PBKDF2
        /// </summary>
        private byte[] DeriveKeyFromPassword(string password, byte[] salt, out byte[] iv)
        {
            // Derive key using PBKDF2
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATION_COUNT))
            {
                byte[] key = pbkdf2.GetBytes(KEY_SIZE); // Key size (32 bytes = 256 bits)
                iv = pbkdf2.GetBytes(IV_SIZE);          // IV size (16 bytes = 128 bits)
                return key;
            }
        }
        
        /// <summary>
        /// Derives a fixed salt from a password for backwards compatibility with old data
        /// </summary>
        private byte[] DeriveFixedSaltFromPassword(string password)
        {
            // Create a fixed salt that's derived from the password itself
            // This is not as secure as a random salt, but allows us to handle old data
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[16]; // 16 byte salt
            
            // Fill the salt with data derived from the password
            for (int i = 0; i < 16; i++)
            {
                salt[i] = i < passwordBytes.Length ? passwordBytes[i] : (byte)i;
            }
            
            return salt;
        }
        
        /// <summary>
        /// Gets a default encryption key
        /// In a real game, you should replace this with your own unique key
        /// </summary>
        private static byte[] GetDefaultKey()
        {
            // Generate a secure random key
            byte[] key = new byte[KEY_SIZE];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }
            return key;
        }
        
        /// <summary>
        /// Gets a default initialization vector
        /// In a real game, you should replace this with your own unique IV
        /// </summary>
        private static byte[] GetDefaultIV()
        {
            // Generate a secure random IV
            byte[] iv = new byte[IV_SIZE];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);
            }
            return iv;
        }
    }
} 