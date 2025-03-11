# EzSave Password-Based Encryption

This document explains how to use the password-based encryption feature in EzSave.

## Overview

EzSave now supports password-based encryption, which provides an additional layer of security for your saved data. When a password is provided, it is used to derive a unique encryption key using a secure key derivation function (PBKDF2).

## Using Password-Based Encryption

### Basic Usage

To use password-based encryption, set both the `EncryptionType` and `Password` properties in your `SaveSettings`:

```csharp
// Create settings with AES encryption and a password
var settings = new SaveSettings("mysave.json")
{
    EncryptionType = EncryptionType.AES,
    Password = "my-secure-password"
};

// Save data with password-based encryption
EzSave.Save("playerData", playerData, settings);

// Load encrypted data (must use the same password)
var loadedData = EzSave.Load<PlayerData>("playerData", settings);
```

### Important Notes

1. **Same Password Required**: When loading data that was saved with a password, you must use the same password to load it. If the wrong password is provided, the decryption will fail.

2. **Password Security**: The password is not stored in the save file. It's only used to derive the encryption key at runtime. However, you should still handle passwords securely in your code.

3. **Fallback to Default Encryption**: If you set `EncryptionType` to `AES` but don't provide a password, EzSave will use the default AES encryption (with a predefined key).

## Constructor Options

EzSave provides several constructors for `SaveSettings` that include password support:

```csharp
// Basic constructor with password
var settings1 = new SaveSettings(
    fileName: "save.json",
    subFolder: "profiles",
    useCompression: false,
    encryptionType: EncryptionType.AES,
    password: "my-secure-password"
);

// Full constructor with password and storage type
var settings2 = new SaveSettings(
    fileName: "save.json",
    subFolder: "profiles",
    useCompression: false,
    encryptionType: EncryptionType.AES,
    password: "my-secure-password",
    storageType: StorageType.FileSystem
);
```

## Security Considerations

1. **Password Strength**: Use strong, unique passwords for better security.

2. **Password Management**: Consider how you'll manage passwords in your game. Will users enter them? Will you generate them? Will they be tied to user accounts?

3. **Password Storage**: Never store passwords in plain text. If you need to persist passwords, consider using a secure storage mechanism like the platform's keychain/credential store.

4. **Salt Generation**: The current implementation generates a random salt for each encryption operation. This means that encrypting the same data with the same password twice will produce different results, which is good for security but means you can't compare encrypted values directly.

## Advanced Usage: Custom Key Derivation

If you need more control over the key derivation process, you can create a custom encryption provider that implements the `IEncryptionProvider` interface and handles passwords in a different way. 