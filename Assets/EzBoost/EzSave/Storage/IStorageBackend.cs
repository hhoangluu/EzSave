using System.Threading.Tasks;

public enum StorageType
{
    FileSystem,
    PlayerPrefs
    // Add other storage types as needed
}

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