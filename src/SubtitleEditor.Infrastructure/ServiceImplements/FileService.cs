using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Infrastructure.Services;
using System.IO.Compression;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class FileService : IFileService
{
    public string StorageFolder { get; }
    public string StorageCacheFolder { get; }
    public string StreamFolder { get; }
    public string WorkspaceFolder { get; }
    public string BenchmarkFolder { get; }
    public string ConfigurationFolder { get; }

    private readonly string _rootPath;
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cacheService;

    private readonly Timer _timer;

    private readonly Dictionary<string, DateTime> _storageCacheMap = new();

    public FileService(
        IWebHostEnvironment env,
        IConfiguration configuration,
        ICacheService cacheService
        )
    {
        _rootPath = env.ContentRootPath;
        _configuration = configuration;

        StorageFolder = Path.Combine(_rootPath, configuration["StorageFolder"], configuration["FileFolder"]);
        StorageCacheFolder = Path.Combine(_rootPath, configuration["StorageFolder"], configuration["StorageCacheFolder"]);
        StreamFolder = Path.Combine(_rootPath, configuration["StorageFolder"], configuration["StreamFolder"]);
        WorkspaceFolder = Path.Combine(_rootPath, configuration["StorageFolder"], configuration["WorkspaceFolder"]);
        BenchmarkFolder = Path.Combine(_rootPath, configuration["StorageFolder"], configuration["BenchmarkFolder"]);
        ConfigurationFolder = Path.Combine(_rootPath, configuration["StorageFolder"], configuration["ConfigurationFolder"]);
        _cacheService = cacheService;

        _timer = new Timer(60000)
        {
            AutoReset = false
        };

        _timer.Elapsed += _timer_Elapsed;
    }

    /// <summary>
    /// 定期掃描 <see cref="_storageCacheMap"/> 內資料的有效期限，如果過期則刪除檔案。
    /// </summary>
    private async void _timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        var now = DateTime.Now;
        foreach (var pair in _storageCacheMap.Where(pair => pair.Value < now))
        {
            if (ExistsInStorageCache(pair.Key))
            {
                await DeleteFromStorageCacheAsync(pair.Key);
            }
        }

        _timer.Start();
    }

    public void InitializeFolders()
    {
        var primaryStorageFolder = Path.Combine(_rootPath, _configuration["StorageFolder"]);
        if (!Directory.Exists(primaryStorageFolder))
        {
            Directory.CreateDirectory(primaryStorageFolder);
        }

        var databaseFolder = Path.Combine(_rootPath, _configuration["StorageFolder"], _configuration["DBFolder"]);
        if (!Directory.Exists(databaseFolder))
        {
            Directory.CreateDirectory(databaseFolder);
        }

        if (!Directory.Exists(StorageFolder))
        {
            Directory.CreateDirectory(StorageFolder);
        }

        if (!Directory.Exists(StorageCacheFolder))
        {
            Directory.CreateDirectory(StorageCacheFolder);
        }

        if (!Directory.Exists(StreamFolder))
        {
            Directory.CreateDirectory(StreamFolder);
        }

        if (!Directory.Exists(WorkspaceFolder))
        {
            Directory.CreateDirectory(WorkspaceFolder);
        }

        if (!Directory.Exists(BenchmarkFolder))
        {
            Directory.CreateDirectory(BenchmarkFolder);
        }

        if (!Directory.Exists(ConfigurationFolder))
        {
            Directory.CreateDirectory(ConfigurationFolder);
        }
    }

    public void InitializeStorageCache()
    {
        // 掃描所有在暫存資料夾內的檔案，如果可以取得檔案的有效期限才進行保留，如果沒辦法取得則直接刪除。
        // 保留的檔案放入 _storageCacheMap 內等待 Timer 定期處理。
        foreach (var filePath in Directory.EnumerateFiles(StorageCacheFolder))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var fileNameArray = fileName.Split("_");
            if (fileNameArray.Length == 2 && double.TryParse(fileNameArray[1], out var oa))
            {
                _storageCacheMap.Add(Path.GetFileName(filePath), DateTime.FromOADate(oa));
            }
        }

        _timer.Start();
    }

    public Task<string> SaveToStorageAsync(Stream stream, string extension)
    {
        return SaveToFolderAsync(StorageFolder, stream, extension);
    }

    public Task<string> SaveToStorageAsync(byte[] data, string extension)
    {
        return SaveToFolderAsync(StorageFolder, data, extension);
    }

    public Task<string> SaveToStorageAsync(Stream stream, string ticket, string extension)
    {
        return SaveToFolderAsync(StorageFolder, stream, ticket, extension);
    }

    public Task<string> SaveToStorageAsync(byte[] data, string ticket, string extension)
    {
        return SaveToFolderAsync(StorageFolder, data, ticket, extension);
    }

    public bool ExistsInStorage(string ticket)
    {
        return ExistsInFolder(StorageFolder, ticket);
    }

    public string GetFilePathInStorage(string ticket)
    {
        return GetFilePathInFolder(StorageFolder, ticket);
    }

    public Task<Stream?> ReadFromStorageAsync(string ticket)
    {
        return ReadFromFolderAsync(StorageFolder, ticket);
    }

    public Task<byte[]?> RetrieveFromStorageAsync(string ticket)
    {
        return RetrieveFromFolderAsync(StorageFolder, ticket);
    }

    public Task<long> ReadLengthFromStorageAsync(string ticket)
    {
        return ReadLengthFromFolder(StorageFolder, ticket);
    }

    public Task DeleteFromStorageAsync(string ticket)
    {
        return DeleteFromFolderAsync(StorageFolder, ticket);
    }

    public bool ExistsInStorageCache(string ticket)
    {
        return ExistsInFolder(StorageCacheFolder, ticket);
    }

    public string GetFilePathInStorageCache(string ticket)
    {
        return GetFilePathInFolder(StorageCacheFolder, ticket);
    }

    public Task<Stream?> ReadFromStorageCacheAsync(string ticket)
    {
        return ReadFromFolderAsync(StorageCacheFolder, ticket);
    }

    public Task<byte[]?> RetrieveFromStorageCacheAsync(string ticket)
    {
        return RetrieveFromFolderAsync(StorageCacheFolder, ticket);
    }

    public Task<long> ReadLengthFromStorageCacheAsync(string ticket)
    {
        return ReadLengthFromFolder(StorageCacheFolder, ticket);
    }

    public Task DeleteFromStorageCacheAsync(string ticket)
    {
        return DeleteFromFolderAsync(StorageCacheFolder, ticket);
    }

    public Task<string> SaveToWorkspaceAsync(Stream stream, string extension)
    {
        return SaveToFolderAsync(WorkspaceFolder, stream, extension);
    }

    public Task<string> SaveToWorkspaceAsync(byte[] data, string extension)
    {
        return SaveToFolderAsync(WorkspaceFolder, data, extension);
    }

    public Task<string> SaveToWorkspaceAsync(Stream stream, string ticket, string extension)
    {
        return SaveToFolderAsync(WorkspaceFolder, stream, ticket, extension);
    }

    public Task<string> SaveToWorkspaceAsync(byte[] data, string ticket, string extension)
    {
        return SaveToFolderAsync(WorkspaceFolder, data, ticket, extension);
    }

    public bool ExistsInWorkspace(string ticket)
    {
        return ExistsInFolder(WorkspaceFolder, ticket);
    }

    public string GetFilePathInWorkspace(string ticket)
    {
        return GetFilePathInFolder(WorkspaceFolder, ticket);
    }

    public Task<Stream?> ReadFromWorkspaceAsync(string ticket)
    {
        return ReadFromFolderAsync(WorkspaceFolder, ticket);
    }

    public Task<byte[]?> RetrieveFromWorkspaceAsync(string ticket)
    {
        return RetrieveFromFolderAsync(WorkspaceFolder, ticket);
    }

    public Task<long> ReadLengthFromWorkspaceAsync(string ticket)
    {
        return ReadLengthFromFolder(WorkspaceFolder, ticket);
    }

    public Task DeleteFromWorkspaceAsync(string ticket)
    {
        return DeleteFromFolderAsync(WorkspaceFolder, ticket);
    }

    public Task<string> SaveToBenchmarkAsync(Stream stream, string extension)
    {
        return SaveToFolderAsync(BenchmarkFolder, stream, extension);
    }

    public Task<string> SaveToBenchmarkAsync(byte[] data, string extension)
    {
        return SaveToFolderAsync(BenchmarkFolder, data, extension);
    }

    public Task<string> SaveToBenchmarkAsync(Stream stream, string ticket, string extension)
    {
        return SaveToFolderAsync(BenchmarkFolder, stream, ticket, extension);
    }

    public Task<string> SaveToBenchmarkAsync(byte[] data, string ticket, string extension)
    {
        return SaveToFolderAsync(BenchmarkFolder, data, ticket, extension);
    }

    public bool ExistsInBenchmark(string ticket)
    {
        return ExistsInFolder(BenchmarkFolder, ticket);
    }

    public string GetFilePathInBenchmark(string ticket)
    {
        return GetFilePathInFolder(BenchmarkFolder, ticket);
    }

    public Task<Stream?> ReadFromBenchmarkAsync(string ticket)
    {
        return ReadFromFolderAsync(BenchmarkFolder, ticket);
    }

    public Task<byte[]?> RetrieveFromBenchmarkAsync(string ticket)
    {
        return RetrieveFromFolderAsync(BenchmarkFolder, ticket);
    }

    public Task<long> ReadLengthFromBenchmarkAsync(string ticket)
    {
        return ReadLengthFromFolder(BenchmarkFolder, ticket);
    }

    public Task DeleteFromBenchmarkAsync(string ticket)
    {
        return DeleteFromFolderAsync(BenchmarkFolder, ticket);
    }

    public async Task ClearBenchmarkFolderAsync()
    {
        foreach (var file in Directory.EnumerateFiles(BenchmarkFolder))
        {
            if (await FileHelper.IsFileSuccessLoadAsync(file))
            {
                File.Delete(file);
            }
        }
    }

    protected Task<string> SaveToFolderAsync(string folder, Stream stream, string extension)
    {
        var ticket = $"{Guid.NewGuid():N}.{extension}";
        return SaveToFolderAsync(folder, ticket, stream);
    }

    protected Task<string> SaveToFolderAsync(string folder, byte[] data, string extension)
    {
        var ticket = $"{Guid.NewGuid():N}.{extension}";
        return SaveToFolderAsync(folder, ticket, data);
    }

    protected Task<string> SaveToFolderAsync(string folder, Stream stream, string ticket, string extension)
    {
        var adoptedTicket = ticket.Contains('.') && ticket.Split('.').Last().Equals(extension, StringComparison.OrdinalIgnoreCase) ? ticket : $"{ticket}.{extension}";
        return SaveToFolderAsync(folder, adoptedTicket, stream);
    }

    protected Task<string> SaveToFolderAsync(string folder, byte[] data, string ticket, string extension)
    {
        var adoptedTicket = ticket.Contains('.') && ticket.Split('.').Last().Equals(extension, StringComparison.OrdinalIgnoreCase) ? ticket : $"{ticket}.{extension}";
        return SaveToFolderAsync(folder, adoptedTicket, data);
    }

    public string GetFilePathInFolder(string folder, string ticket)
    {
        return Path.Combine(folder, ticket);
    }

    public bool ExistsInFolder(string folder, string ticket)
    {
        var path = Path.Combine(folder, ticket);
        if (string.IsNullOrWhiteSpace(ticket) || !File.Exists(path))
        {
            return false;
        }

        return File.Exists(path);
    }

    public async Task<Stream?> ReadFromFolderAsync(string folder, string ticket)
    {
        var path = Path.Combine(folder, ticket);
        if (string.IsNullOrWhiteSpace(ticket) || !File.Exists(path) || !await FileHelper.IsFileSuccessLoadAsync(path))
        {
            return null;
        }

        return File.OpenRead(path);
    }

    public async Task<byte[]?> RetrieveFromFolderAsync(string folder, string ticket)
    {
        var path = Path.Combine(folder, ticket);
        if (string.IsNullOrWhiteSpace(ticket) || !File.Exists(path) || !await FileHelper.IsFileSuccessLoadAsync(path))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(path);
    }

    public async Task<long> ReadLengthFromFolder(string folder, string ticket)
    {
        var path = Path.Combine(folder, ticket);
        if (string.IsNullOrWhiteSpace(ticket) || !File.Exists(path) || !await FileHelper.IsFileSuccessLoadAsync(path))
        {
            return 0;
        }

        return new FileInfo(path).Length;
    }

    public async Task DeleteFromFolderAsync(string folder, string ticket)
    {
        var path = Path.Combine(folder, ticket);
        if (string.IsNullOrWhiteSpace(ticket) || !File.Exists(path) || !await FileHelper.IsFileSuccessLoadAsync(path))
        {
            return;
        }

        File.Delete(path);
    }

    public Task<string> SaveToCacheAsync(Stream stream, string extension)
    {
        return SaveToCacheAsync(stream, extension, due: default);
    }

    public Task<string> SaveToCacheAsync(Stream stream, string ticket, string extension)
    {
        return SaveToCacheAsync(stream, ticket, extension, default);
    }

    public Task<string> SaveToCacheAsync(Stream stream, string extension, DateTime due)
    {
        var ticket = $"{Guid.NewGuid():N}_{DateTime.Now.AddHours(8).ToOADate()}{(!string.IsNullOrEmpty(extension) ? $".{extension}" : "")}";
        return SaveToCacheAsync(stream, ticket, extension, due);
    }

    public async Task<string> SaveToCacheAsync(Stream stream, string ticket, string extension, DateTime due)
    {
        //// 如果輸入的檔案小於 512MB，則存在記憶體中；如果超過則存到硬碟，並設定保存期限。
        //if (stream.Length <= 536870912) // 512MB
        //{
        //    using var memoryStream = new MemoryStream();
        //    stream.Position = 0;
        //    stream.CopyTo(memoryStream);

        //    return SaveToCache(memoryStream.ToArray(), ticket, extension, due);
        //}
        //else
        //{
        //    // 暫存在硬碟的檔案，檔名除了原本的 GUID 外，還須包含有效期限 (yyyy-MM-dd)。
        //    var adoptedTicket = ticket.Split('.').Length == 2 ? ticket : $"{ticket}.{extension}";
        //    await SaveToFolderAsync(StorageCacheFolder, adoptedTicket, stream);
        //    return ticket;
        //}
        // 暫存在硬碟的檔案，檔名除了原本的 GUID 外，還須包含有效期限 (yyyy-MM-dd)。
        var adoptedTicket = ticket.Contains('.') && ticket.Split('.').Last().Equals(extension, StringComparison.OrdinalIgnoreCase) ? ticket : $"{ticket}.{extension}";
        await SaveToFolderAsync(StorageCacheFolder, adoptedTicket, stream);
        return ticket;
    }

    public string SaveToCache(byte[] data, string extension)
    {
        return SaveToCache(data, extension, due: default);
    }

    public string SaveToCache(byte[] data, string ticket, string extension)
    {
        return SaveToCache(data, ticket, extension, default);
    }

    public string SaveToCache(byte[] data, string extension, DateTime due)
    {
        var ticket = $"{Guid.NewGuid():N}{(!string.IsNullOrEmpty(extension) ? $".{extension}" : "")}";
        SaveToCache(ticket, data, due);
        return ticket;
    }

    public string SaveToCache(byte[] data, string ticket, string extension, DateTime due)
    {
        var adoptedTicket = ticket.Contains('.') && ticket.Split('.').Last().Equals(extension, StringComparison.OrdinalIgnoreCase) ? ticket : $"{ticket}.{extension}";
        SaveToCache(adoptedTicket, data, due);
        return ticket;
    }

    public bool ExistsInCache(string ticket)
    {
        return _cacheService.ContainsKey(ticket) || ExistsInStorageCache(ticket);
    }

    public async Task<byte[]?> RetrieveFromCacheAsync(string ticket)
    {
        if (_cacheService.ContainsKey(ticket))
        {
            return _cacheService.Get<byte[]>(ticket);
        }
        else if (ExistsInStorageCache(ticket))
        {
            return await RetrieveFromStorageCacheAsync(ticket);
        }

        return null;
    }

    public async Task<Stream?> ReadFromCacheAsync(string ticket)
    {
        if (_cacheService.ContainsKey(ticket))
        {
            var data = _cacheService.Get<byte[]>(ticket);
            return data != null ? new MemoryStream(data) : null;
        }
        else if (ExistsInStorageCache(ticket))
        {
            return await ReadFromStorageCacheAsync(ticket);
        }

        return null;
    }

    public async Task<long> ReadLengthFromCacheAsync(string ticket)
    {
        if (_cacheService.ContainsKey(ticket))
        {
            var data = _cacheService.Get<byte[]>(ticket);
            return data?.Length ?? 0;
        }
        else if (ExistsInStorageCache(ticket))
        {
            return await ReadLengthFromStorageCacheAsync(ticket);
        }

        return 0;
    }

    public void DeleteCache(string ticket)
    {
        _cacheService.Remove(ticket);
    }

    public Task<MemoryStream> ConvertTextToFileStreamAsync(string text)
    {
        return ConvertTextToFileStreamAsync(text, Encoding.Unicode);
    }

    public Task<byte[]> ConvertTextToFileDataAsync(string text)
    {
        return ConvertTextToFileDataAsync(text, Encoding.Unicode);
    }

    public async Task<MemoryStream> ConvertTextToFileStreamAsync(string text, Encoding encoding)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, encoding);
        await writer.WriteAsync(text);
        await writer.FlushAsync();
        stream.Position = 0;
        return stream;
    }

    public async Task<byte[]> ConvertTextToFileDataAsync(string text, Encoding encoding)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, encoding);
        await writer.WriteAsync((text.Contains("\r\n") ? text.Replace("\r\n", "\n") : text).Replace("\n", "\r\n"));
        await writer.FlushAsync();
        stream.Position = 0;
        return stream.ToArray();
    }

    public async Task<MemoryStream> BundleFilesAsync(Dictionary<string, byte[]> files)
    {
        using var compressedFileStream = new MemoryStream();
        using var zip = new ZipArchive(compressedFileStream, ZipArchiveMode.Update, true);

        foreach (var file in files)
        {
            var zipEntry = zip.CreateEntry(file.Key);

            using var zipEntryStream = zipEntry.Open();
            await zipEntryStream.WriteAsync(file.Value, 0, file.Value.Length);
            zipEntryStream.Position = 0;
        }

        return compressedFileStream;
    }

    public async Task<string> TransferFileFromCacheToStorageAsync(string ticket, string extension)
    {
        if (StorageFolder == StorageCacheFolder && ExistsInStorageCache(ticket))
        {
            var adoptedTicket = $"{ticket.Split('_').First()}.{extension}";
            File.Move(Path.Combine(StorageCacheFolder, ticket), Path.Combine(StorageFolder, adoptedTicket));
            return adoptedTicket;
        }
        else
        {
            using var stream = await ReadFromCacheAsync(ticket) ?? throw new Exception("File not found in cache");
            return await SaveToStorageAsync(stream, extension);
        }
    }

    public long GetTotalLengthOfStorage()
    {
        return Directory.EnumerateFiles(StorageFolder)
            .Select(filePath => new FileInfo(filePath).Length)
            .Sum();
    }

    public long GetTotalLengthOfStreamStorage()
    {
        return Directory.EnumerateFiles(StreamFolder, "*", SearchOption.AllDirectories)
            .Select(filePath => new FileInfo(filePath).Length)
            .Sum();
    }

    protected static async Task<string> SaveToFolderAsync(string folder, string ticket, Stream stream)
    {
        using var fileStream = File.Create(Path.Combine(folder, ticket));
        stream.Seek(0, SeekOrigin.Begin);
        await stream.CopyToAsync(fileStream);
        return ticket;
    }

    protected static async Task<string> SaveToFolderAsync(string folder, string ticket, byte[] data)
    {
        await File.WriteAllBytesAsync(Path.Combine(folder, ticket), data);
        return ticket;
    }

    protected void SaveToCache(string ticket, byte[] data, DateTime due)
    {
        if (due == default)
        {
            _cacheService.Set(ticket, data);
        }
        else
        {
            _cacheService.Set(ticket, due - DateTime.Now, data);
        }
    }
}
