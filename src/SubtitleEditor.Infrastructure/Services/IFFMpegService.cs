using SubtitleEditor.Infrastructure.Models.FFMpeg;

namespace SubtitleEditor.Infrastructure.Services;

public interface IFFMpegService
{
    Task DetermineFFMpegSourceAsync();
    Task<string[]> ListHardwareAccelerationAsync();
    Task<ConvertToM3U8Result> ConvertToM3U8Async(string sourceFilePath, CancellationToken stoppingToken = default);
    Task<ConvertToM3U8Result> ConvertToM3U8Async(string sourceFilePath, string argumentTemplate, CancellationToken stoppingToken = default);
    Task<double> GetDurationAsync(string sourceFilePath, CancellationToken stoppingToken = default);
}
