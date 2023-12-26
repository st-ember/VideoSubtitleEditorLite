using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Database;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Worker.Infrastructure.Services;

namespace SubtitleEditor.Worker.Infrastructure.ServiceImplements;

public class StreamConvertService : IStreamConvertService
{
    private readonly EditorContext _database;
    private readonly ILogService _logService;
    private readonly IFileService _fileService;
    private readonly IFFMpegService _ffMpegService;
    private readonly IStreamFileService _streamFileService;

    public StreamConvertService(
        IServiceProvider serviceProvider,
        ILogService logService, 
        IFileService fileService,
        IFFMpegService ffMpegService,
        IStreamFileService streamFileService
        )
    {
        _database = serviceProvider.GetDatabase();
        _logService = logService;
        _fileService = fileService;
        _ffMpegService = ffMpegService;
        _streamFileService = streamFileService;
    }

    public async Task ProcessNextAsync(CancellationToken cancellationToken)
    {
        var waitingAndProcessingTopics = await _database.Topics
            .AsNoTracking()
            .Include(e => e.Media)
            .Where(e => e.Media.ConvertStatus == ConvertMediaStatus.FFMpegWaiting || e.Media.ConvertStatus == ConvertMediaStatus.FFMpegProcessing)
            .ToArrayAsync(cancellationToken);

        await _setWaitingAsync(waitingAndProcessingTopics.Where(e => e.Media.ConvertStatus == ConvertMediaStatus.FFMpegProcessing), cancellationToken);

        var priorityTopic = waitingAndProcessingTopics
            .Where(e => e.Status == TopicStatus.Normal && e.Media.ConvertStatus == ConvertMediaStatus.FFMpegWaiting)
            .OrderByDescending(e => e.Priority).ThenBy(e => e.Create)
            .FirstOrDefault();

        if (priorityTopic != null)
        {
            _logService.Target = priorityTopic.Name;

            if (priorityTopic.Media.Ticket == null)
            {
                await _setErrorAsync(priorityTopic, "檔案 Ticket 無效。", cancellationToken);
                return;
            }

            if (!_fileService.ExistsInStorage(priorityTopic.Media.Ticket))
            {
                await _setErrorAsync(priorityTopic, $"存放區中的檔案 {priorityTopic.Media.Ticket} 無效。", cancellationToken);
                return;
            }

            var filePath = _fileService.GetFilePathInStorage(priorityTopic.Media.Ticket);
            await _setProcessingAsync(priorityTopic, cancellationToken);

            var convertResult = await _ffMpegService.ConvertToM3U8Async(filePath, cancellationToken);
            if (convertResult.OutputFilePaths.Length == 0)
            {
                await _setErrorAsync(priorityTopic, "沒有 m3u8 檔案產出。", cancellationToken);
                return;
            }

            long streamSize = 0;
            foreach (var streamFilePath in convertResult.OutputFilePaths)
            {
                if (!await FileHelper.IsFileSuccessLoadAsync(streamFilePath))
                {
                    await _setErrorAsync(priorityTopic, $"檔案被占用導致無法讀取：{streamFilePath}", cancellationToken);
                    return;
                }

                var data = await File.ReadAllBytesAsync(streamFilePath, cancellationToken);
                if (data == null || data.LongLength == 0)
                {
                    await _setErrorAsync(priorityTopic, $"讀取檔案失敗：{streamFilePath}", cancellationToken);
                    return;
                }

                streamSize += data.LongLength;

                try
                {
                    await _streamFileService.SaveFileAsync(priorityTopic.Id.ToString(), data, Path.GetFileName(streamFilePath), cancellationToken);
                }
                catch (Exception ex)
                {
                    await _setErrorAsync(priorityTopic, $"存放檔案失敗({streamFilePath})：{ex}", cancellationToken);
                    return;
                }
            }

            if (priorityTopic.AsrTaskId.HasValue)
            {
                await _fileService.DeleteFromStorageAsync(priorityTopic.Media.Ticket);
            }
            
            await _setCompletedAsync(priorityTopic, streamSize, cancellationToken);

            if (priorityTopic.Media.AsrStatus == AsrMediaStatus.ASRCompleted && priorityTopic.Media.Ticket != null)
            {
                // 完成後如果有跑 Asr 且完成了，把檔案刪除。
                await _fileService.DeleteFromStorageAsync(priorityTopic.Media.Ticket);
            }
        }
    }

    private async Task _setWaitingAsync(IEnumerable<Topic> topics, CancellationToken cancellationToken)
    {
        if (!topics.Any())
        {
            return;
        }

        foreach (var topic in topics)
        {
            _database.Attach(topic);
            topic.Media.Progress = 0;
            topic.Media.ConvertStatus = ConvertMediaStatus.FFMpegWaiting;
        }

        await _database.SaveChangesAsync(cancellationToken);
        _database.Detach(entities: topics);
    }

    private async Task _setProcessingAsync(Topic topic, CancellationToken cancellationToken)
    {
        _database.Attach(topic);
        topic.Media.Progress = 0;
        topic.Media.Size = 0;
        topic.Media.ConvertStatus = ConvertMediaStatus.FFMpegProcessing;
        topic.Media.ProcessStart = DateTime.Now;
        topic.Media.ProcessEnd = null;
        topic.Media.Update = DateTime.Now;
        await _database.SaveChangesAsync(cancellationToken);
        _database.Detach(topic);
    }

    private async Task _setCompletedAsync(Topic topic, long streamSize, CancellationToken cancellationToken)
    {
        _database.Attach(topic);

        topic.Media.Progress = 0;
        topic.Media.Size = streamSize;
        topic.Media.ConvertStatus = ConvertMediaStatus.FFMpegCompleted;
        topic.Media.ProcessEnd = DateTime.Now;
        topic.Media.Update = DateTime.Now;
        topic.Update = DateTime.Now;

        await _database.SaveChangesAsync(cancellationToken);
        _database.Detach(topic);
    }

    private async Task _setErrorAsync(Topic topic, string error, CancellationToken cancellationToken)
    {
        _database.Attach(topic);
        topic.Media.ConvertStatus = ConvertMediaStatus.FFMpegFailed;
        topic.Media.Size = 0;
        topic.Media.Error = error;
        topic.Media.ProcessEnd = DateTime.Now;
        topic.Media.Update = DateTime.Now;

        await _database.SaveChangesAsync(cancellationToken);
        _database.Detach(topic);

        throw new Exception(error);
    }
}
