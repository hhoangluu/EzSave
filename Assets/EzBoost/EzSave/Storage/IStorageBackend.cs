using System.Threading.Tasks;

/// <summary>
/// Storage type enum for different backend options
/// </summary>
public enum StorageType
{
    FileSystem,
    PlayerPrefs
    // Add other storage types as needed
}

/// <summary>
/// Interface for storage backend operations
/// </summary>
public interface IStorageBackend
{
    bool FileExists(string filePath);
    string ReadFile(string filePath);
    void WriteFile(string filePath, string contents);
    bool DeleteFile(string filePath);
    bool DeleteAllFiles();
    Task<string> ReadFileAsync(string filePath);
    Task WriteFileAsync(string filePath, string contents);
    Task<bool> DeleteFileAsync(string filePath);
    Task<bool> DeleteAllFilesAsync();
    string GetBasePath();
}