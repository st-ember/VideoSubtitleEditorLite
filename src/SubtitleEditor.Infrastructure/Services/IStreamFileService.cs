namespace SubtitleEditor.Infrastructure.Services;

public interface IStreamFileService
{
    string StreamFolder { get; }

    Task SaveFileAsync(string streamId, byte[] data, string filenameWithExtension, CancellationToken? cancellationToken = null);
    bool Exists(string streamId);
    Task<string?> RetrieveM3U8Async(string streamId, CancellationToken? cancellationToken = null);
    Stream? RetrieveTsFile(string streamId, string number, CancellationToken? cancellationToken = null);
    void DeleteFromStreamFolder(string streamId);
}
