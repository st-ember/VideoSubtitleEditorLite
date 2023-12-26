using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Database;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.Infrastructure.Models.Benchmark;
using SubtitleEditor.Web.Infrastructure.Services;

namespace SubtitleEditor.Web.Infrastructure.ServiceImplements;

public class BenchmarkService : IBenchmarkService
{
    private readonly EditorContext _database;
    private readonly IFileService _fileService;
    private readonly ILogService _logService;
    private readonly IAsrService _asrService;
    private readonly IFFMpegService _ffMpegService;

    private readonly string _argumentTemplate;

    public BenchmarkService(
        IConfiguration configuration,
        EditorContext database,
        IFileService fileService,
        ILogService logService,
        IAsrService asrService,
        IFFMpegService ffMpegService
        )
    {
        _database = database;
        _fileService = fileService;
        _logService = logService;
        _asrService = asrService;
        _ffMpegService = ffMpegService;

        _argumentTemplate = configuration["StreamConverter:ArgumentTemplate"];
    }

    public async Task<BenchmarkModel> DoBenchmarkAsync(Guid topicId, string? argumentTemplate = null)
    {
        _logService.Target = topicId.ToString();

        var topic = await _database.Topics
            .AsNoTracking()
            .Include(e => e.Media)
            .Where(e => e.Id == topicId && e.Status != TopicStatus.Removed)
            .FirstOrDefaultAsync() ?? throw new Exception("找不到指定的單集。");

        _logService.Target = topic.Name;

        var existedProcess = await _database.Topics
            .AsNoTracking()
            .Include(e => e.Media)
            .AnyAsync(e => e.Status != TopicStatus.Removed &&
                (e.Media.ConvertStatus == ConvertMediaStatus.FFMpegWaiting || e.Media.ConvertStatus == ConvertMediaStatus.FFMpegProcessing)
                );

        if (existedProcess)
        {
            throw new Exception("已有轉檔工作在進行中，請等待現有工作完成後再進行測試。");
        }

        var benchmark = new BenchmarkModel()
        {
            ArgumentTemplate = argumentTemplate ?? _argumentTemplate,
            Length = topic.Media.Length
        };

        var benchmarkFileTicket = string.Empty;

        if (topic.AsrTaskId.HasValue)
        {
            var link = await _asrService.GetTaskFileLinkAsync(topic.AsrTaskId.Value);
            if (string.IsNullOrWhiteSpace(link))
            {
                throw new Exception("無法取得遠端連結。");
            }

            await benchmark.DoRetrieveRawFileFromAsr(async () =>
            {
                using var stream = await _asrService.RetrieveFileAsync(link);
                benchmarkFileTicket = await _fileService.SaveToBenchmarkAsync(stream, topic.Media.Extension);
            });
        }
        else
        {
            if (topic.Media == null)
            {
                throw new Exception("此單集沒有有效的媒體資料。");
            }

            if (string.IsNullOrWhiteSpace(topic.Media.Ticket))
            {
                throw new Exception("此單集沒有有效的媒體檔案。");
            }

            if (!_fileService.ExistsInStorage(topic.Media.Ticket))
            {
                throw new Exception("媒體檔案遺失。");
            }

            await benchmark.DoRetrieveRawFileFromLocal(async () =>
            {
                using var stream = (await _fileService.ReadFromStorageAsync(topic.Media.Ticket))!;
                benchmarkFileTicket = await _fileService.SaveToBenchmarkAsync(stream, topic.Media.Extension);
            });
        }

        await benchmark.DoConvertFile(async () =>
        {
            var result = await _ffMpegService.ConvertToM3U8Async(_fileService.GetFilePathInBenchmark(benchmarkFileTicket), benchmark.ArgumentTemplate);
            benchmark.Output = result.Output;
            benchmark.Success = result.OutputFilePaths.Any();
        });

        await _fileService.ClearBenchmarkFolderAsync();

        return benchmark;
    }
}