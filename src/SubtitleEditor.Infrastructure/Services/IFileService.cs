using System.Text;

namespace SubtitleEditor.Infrastructure.Services;

public interface IFileService
{
    string StorageFolder { get; }
    string StreamFolder { get; }
    string WorkspaceFolder { get; }
    string BenchmarkFolder { get; }
    string ConfigurationFolder { get; }

    void InitializeFolders();
    void InitializeStorageCache();

    Task<string> SaveToStorageAsync(Stream stream, string extension);
    Task<string> SaveToStorageAsync(byte[] data, string extension);
    Task<string> SaveToStorageAsync(Stream stream, string ticket, string extension);
    Task<string> SaveToStorageAsync(byte[] data, string ticket, string extension);
    bool ExistsInStorage(string ticket);
    string GetFilePathInStorage(string ticket);
    Task<Stream?> ReadFromStorageAsync(string ticket);
    Task<byte[]?> RetrieveFromStorageAsync(string ticket);
    Task<long> ReadLengthFromStorageAsync(string ticket);
    Task DeleteFromStorageAsync(string ticket);

    Task<string> SaveToWorkspaceAsync(Stream stream, string extension);
    Task<string> SaveToWorkspaceAsync(byte[] data, string extension);
    Task<string> SaveToWorkspaceAsync(Stream stream, string ticket, string extension);
    Task<string> SaveToWorkspaceAsync(byte[] data, string ticket, string extension);
    bool ExistsInWorkspace(string ticket);
    string GetFilePathInWorkspace(string ticket);
    Task<Stream?> ReadFromWorkspaceAsync(string ticket);
    Task<byte[]?> RetrieveFromWorkspaceAsync(string ticket);
    Task<long> ReadLengthFromWorkspaceAsync(string ticket);
    Task DeleteFromWorkspaceAsync(string ticket);

    Task<string> SaveToBenchmarkAsync(Stream stream, string extension);
    Task<string> SaveToBenchmarkAsync(byte[] data, string extension);
    Task<string> SaveToBenchmarkAsync(Stream stream, string ticket, string extension);
    Task<string> SaveToBenchmarkAsync(byte[] data, string ticket, string extension);
    bool ExistsInBenchmark(string ticket);
    string GetFilePathInBenchmark(string ticket);
    Task<Stream?> ReadFromBenchmarkAsync(string ticket);
    Task<byte[]?> RetrieveFromBenchmarkAsync(string ticket);
    Task<long> ReadLengthFromBenchmarkAsync(string ticket);
    Task DeleteFromBenchmarkAsync(string ticket);
    Task ClearBenchmarkFolderAsync();

    Task<string> SaveToCacheAsync(Stream stream, string extension);
    Task<string> SaveToCacheAsync(Stream stream, string ticket, string extension);
    Task<string> SaveToCacheAsync(Stream stream, string extension, DateTime due);
    Task<string> SaveToCacheAsync(Stream stream, string ticket, string extension, DateTime due);

    string SaveToCache(byte[] data, string extension);
    string SaveToCache(byte[] data, string ticket, string extension);
    string SaveToCache(byte[] data, string extension, DateTime due);
    string SaveToCache(byte[] data, string ticket, string extension, DateTime due);

    bool ExistsInCache(string ticket);

    Task<byte[]?> RetrieveFromCacheAsync(string ticket);
    Task<Stream?> ReadFromCacheAsync(string ticket);
    Task<long> ReadLengthFromCacheAsync(string ticket);

    void DeleteCache(string ticket);

    Task<MemoryStream> ConvertTextToFileStreamAsync(string text);
    Task<byte[]> ConvertTextToFileDataAsync(string text);
    Task<MemoryStream> ConvertTextToFileStreamAsync(string text, Encoding encoding);
    Task<byte[]> ConvertTextToFileDataAsync(string text, Encoding encoding);
    Task<MemoryStream> BundleFilesAsync(Dictionary<string, byte[]> files);
    Task<string> TransferFileFromCacheToStorageAsync(string ticket, string extension);

    long GetTotalLengthOfStorage();
    long GetTotalLengthOfStreamStorage();
}
