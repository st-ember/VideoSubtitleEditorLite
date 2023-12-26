using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class StreamFileService : IStreamFileService
{
    public string StreamFolder { get; }

    private readonly ICacheService _cacheService;

    public StreamFileService(
        IWebHostEnvironment env,
        IConfiguration configuration,
        ICacheService cacheService
        )
    {
        StreamFolder = Path.Combine(env.ContentRootPath, configuration["StorageFolder"], configuration["StreamFolder"]);
        _cacheService = cacheService;
    }

    public Task SaveFileAsync(string streamId, byte[] data, string filenameWithExtension, CancellationToken? cancellationToken = null)
    {
        var folderPath = Path.Combine(StreamFolder, streamId);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filename;
        if (filenameWithExtension.EndsWith("m3u8"))
        {
            filename = $"{streamId}.m3u8";
        }
        else if (filenameWithExtension.EndsWith("ts"))
        {
            var number = int.TryParse(filenameWithExtension.Split('.').First().Split('_').Last(), out var i) ? i : -1;
            if (number >= 0)
            {
                filename = $"{streamId}_{number}.ts";
            }
            else
            {
                throw new Exception($"ts 檔案格式錯誤，錯誤的檔名為：{filenameWithExtension}");
            }
        }
        else
        {
            throw new Exception($"檔案格式錯誤，錯誤的檔名為：{filenameWithExtension}");
        }

        if (cancellationToken.HasValue)
        {
            return File.WriteAllBytesAsync(Path.Combine(folderPath, filename), data, cancellationToken.Value);
        }
        else
        {
            return File.WriteAllBytesAsync(Path.Combine(folderPath, filename), data);
        }
    }

    public bool Exists(string streamId)
    {
        var folderPath = Path.Combine(StreamFolder, streamId);
        if (!Directory.Exists(folderPath))
        {
            return false;
        }

        return File.Exists(Path.Combine(folderPath, $"{streamId}.m3u8"));
    }

    public async Task<string?> RetrieveM3U8Async(string streamId, CancellationToken? cancellationToken = null)
    {
        var key = $"m3u8File-{streamId}.m3u8";
        var result = await _cacheService.GetOrCreateAsync(key, TimeSpan.FromHours(1), async () =>
        {
            var folderPath = Path.Combine(StreamFolder, streamId);
            if (!Directory.Exists(folderPath))
            {
                return null;
            }

            var filePath = Path.Combine(folderPath, $"{streamId}.m3u8");
            if (!Directory.Exists(folderPath))
            {
                return null;
            }

            return cancellationToken.HasValue ?
                await File.ReadAllTextAsync(filePath, cancellationToken.Value) :
                await File.ReadAllTextAsync(filePath);
        });

        if (result == null)
        {
            _cacheService.Remove(key);
        }

        return result;
    }

    public Stream? RetrieveTsFile(string streamId, string number, CancellationToken? cancellationToken = null)
    {
        var folderPath = Path.Combine(StreamFolder, streamId);
        if (!Directory.Exists(folderPath))
        {
            return null;
        }

        var filePath = Path.Combine(folderPath, $"{streamId}_{number}.ts");
        if (!File.Exists(filePath))
        {
            return null;
        }

        return File.OpenRead(filePath);
    }

    public void DeleteFromStreamFolder(string streamId)
    {
        var folderPath = Path.Combine(StreamFolder, streamId);
        if (!Directory.Exists(folderPath))
        {
            return;
        }

        Directory.Delete(folderPath, true);
    }
}
