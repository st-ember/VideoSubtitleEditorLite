using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Database;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Helpers;
using SubtitleEditor.Infrastructure.Models.Asr;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Worker.Infrastructure.Services;
using Subtitle = SubtitleEditor.Database.Entities.Subtitle;

namespace SubtitleEditor.Worker.Infrastructure.ServiceImplements;

public class AsrProcessService : IAsrProcessService
{
    private readonly EditorContext _database;
    private readonly ILogService _logService;
    private readonly IAsrService _asrService;
    private readonly IFileService _fileService;
    private readonly ISubtitleService _subtitleService;
    private readonly IFixBookService _fixBookService;

    public AsrProcessService(
        IServiceProvider serviceProvider,
        ILogService logService,
        IAsrService asrService,
        IFileService fileService,
        ISubtitleService subtitleService,
        IFixBookService fixBookService
        )
    {
        _database = serviceProvider.GetDatabase();
        _logService = logService;
        _asrService = asrService;
        _fileService = fileService;
        _subtitleService = subtitleService;
        _fixBookService = fixBookService;
    }

    public async Task ProcessNextAsync(CancellationToken cancellationToken)
    {
        var waitingAndProcessingTopics = await _database.Topics
            .Include(e => e.Media)
            .Where(e => e.Media.AsrStatus == AsrMediaStatus.ASRWaiting || e.Media.AsrStatus == AsrMediaStatus.ASRProcessing)
            .ToArrayAsync(cancellationToken);

        if (waitingAndProcessingTopics.Any(e => e.Media.AsrStatus == AsrMediaStatus.ASRProcessing))
        {
            foreach (var topic in waitingAndProcessingTopics.Where(e => e.Media.AsrStatus == AsrMediaStatus.ASRProcessing))
            {
                await _checkAndUpdateProcessingTopicStatusAsync(topic, cancellationToken);
            }

            await _database.SaveChangesAsync(cancellationToken);
            Console.WriteLine("尚有 ASR 工作進行中。");
            return;
        }

        var priorityTopic = waitingAndProcessingTopics
            .Where(e => e.Status == TopicStatus.Normal && e.Media.AsrStatus == AsrMediaStatus.ASRWaiting)
            .OrderByDescending(e => e.Priority).ThenBy(e => e.Create)
            .FirstOrDefault();

        if (priorityTopic != null)
        {
            _logService.Target = priorityTopic.Name;

            if (priorityTopic.Media.Ticket == null)
            {
                await _setErrorAsync(priorityTopic, "找不到存放區中的檔案 Ticket。", cancellationToken);
                return;
            }

            var stream = await _fileService.ReadFromStorageAsync(priorityTopic.Media.Ticket);
            if (stream == null || stream.Length == 0)
            {
                await _setErrorAsync(priorityTopic, $"存放區中的檔案 {priorityTopic.Media.Ticket} 無效。", cancellationToken);
                return;
            }

            try
            {
                var taskId = await _asrService.CreateTaskAsync(new NctuCreateTaskRequest
                {
                    Filename = priorityTopic.Media.Filename,
                    Stream = stream,
                    ModelName = priorityTopic.GetModelName()
                }, cancellationToken);

                await _setStartProcessAsync(priorityTopic, taskId, stream.Length, cancellationToken);
            }
            catch (Exception ex)
            {
                await _setErrorAsync(priorityTopic, $"建立 ASR 工作失敗：{ex}", cancellationToken);
            }
        }
    }

    private async Task _checkAndUpdateProcessingTopicStatusAsync(Topic topic, CancellationToken cancellationToken)
    {
        if (topic.AsrTaskId.HasValue)
        {
            var asrTask = await _asrService.GetTaskAsync(topic.AsrTaskId.Value, cancellationToken);
            if (asrTask != null)
            {
                topic.SetAsrTaskData(asrTask);

                if (asrTask.Status == NctuTaskStatus.Processing)
                {
                    await _setProcessingAsync(topic, asrTask.Progress, asrTask.AudioLength, cancellationToken);
                }
                else if (asrTask.Status == NctuTaskStatus.Canceled)
                {
                    await _setCanceledAsync(topic, cancellationToken);
                }
                else if (asrTask.Status == NctuTaskStatus.Failed)
                {
                    await _setFailedAsync(topic, asrTask, cancellationToken);
                }
                else if (asrTask.Status == NctuTaskStatus.Succeeded)
                {
                    var subtitle = await _retrieveAsrResultAsync(topic, cancellationToken);
                    await _addTopicSubtitleAsync(topic, subtitle, cancellationToken);
                    await _setSucceededAsync(topic, cancellationToken);
                }
                else if (asrTask.Status != NctuTaskStatus.Checking && topic.Media.ASRProcessStart.HasValue && DateTime.Now - topic.Media.ASRProcessStart.Value > TimeSpan.FromHours(8))
                {
                    await _setErrorAsync(topic, "ASR 工作已逾時。", cancellationToken);
                }
            }
            else
            {
                topic.ClearAsrTaskData();
                await _setErrorAsync(topic, "ASR 工作已被刪除。", cancellationToken);
            }
        }
        else
        {
            topic.ClearAsrTaskData();
            await _setErrorAsync(topic, "無法對應 ASR 工作。", cancellationToken);
        }
    }

    private async Task<Core.Models.Subtitle> _retrieveAsrResultAsync(Topic topic, CancellationToken cancellationToken)
    {
        var taskId = topic.AsrTaskId!.Value;
        var subtitleLink = await _asrService.GetSubtitleLinkAsync(taskId, cancellationToken);
        var subtitle = _subtitleService.ParseFromSrt(await _asrService.RetrieveTextFileAsync(subtitleLink["SRT"], cancellationToken));
        subtitle.Srt = _subtitleService.ToSrt(subtitle);

        var nctuWordSegments = await _asrService.GetTaskWordSegmentsAsync(taskId, cancellationToken);
        subtitle = _subtitleService.UpdateSubtitleWithWordSegments(subtitle, nctuWordSegments);

        var fixBookData = await _fixBookService.GetByModelAsync();
        _subtitleService.ReplaceInSubtitle(subtitle, fixBookData.Items.ToDictionary(m => m.Original, m => m.Correction));

        return subtitle;
    }

    private async Task _setProcessingAsync(Topic topic, int progress, int length, CancellationToken cancellationToken)
    {
        _database.Attach(topic);
        topic.Media.Progress = progress;
        topic.Media.Length = length;
        topic.Media.Error = null;
        topic.Media.Update = DateTime.Now;
        await _database.SaveChangesAsync(cancellationToken);
        _database.Detach(topic);
    }

    private async Task _setCanceledAsync(Topic topic, CancellationToken cancellationToken)
    {
        _database.Attach(topic);
        topic.Media.AsrStatus = AsrMediaStatus.ASRCanceled;
        topic.Media.ASRProcessEnd = DateTime.Now;
        topic.Media.Update = DateTime.Now;
        await _database.SaveChangesAsync(cancellationToken);
        _database.Detach(topic);
    }

    private async Task _setFailedAsync(Topic topic, NctuTask nctuTask, CancellationToken cancellationToken)
    {
        _database.Attach(topic);
        topic.Media.AsrStatus = AsrMediaStatus.ASRFailed;
        topic.Media.ASRProcessEnd = DateTime.Now;
        topic.Media.Update = DateTime.Now;
        topic.Media.Error = nctuTask.ResultComment;
        await _database.SaveChangesAsync(cancellationToken);
        _database.Detach(topic);
    }

    private async Task _setSucceededAsync(Topic topic, CancellationToken cancellationToken)
    {
        _database.Attach(topic);

        topic.Media.ASRProcessEnd = DateTime.Now;
        topic.Media.Update = DateTime.Now;

        topic.Media.AsrStatus = AsrMediaStatus.ASRCompleted;

        if (topic.Media.ConvertStatus == ConvertMediaStatus.FFMpegCompleted && topic.Media.Ticket != null)
        {
            // 如果檔案已經轉檔完成，刪掉。
            await _fileService.DeleteFromStorageAsync(topic.Media.Ticket);
        }

        await _database.SaveChangesAsync(cancellationToken);
        _database.Detach(topic);
    }

    private async Task _addTopicSubtitleAsync(Topic topic, Core.Models.Subtitle subtitle, CancellationToken cancellationToken)
    {
        var entity = await _database.Subtitles
            .Where(e => e.TopicId == topic.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            entity = new Subtitle
            {
                TopicId = topic.Id
            };
            _database.Subtitles.Add(entity);
        }

        entity.SetOrigianlSubtitle(subtitle);
        entity.SetSubtitle(subtitle);

        await _database.SaveChangesAsync(cancellationToken);
        _database.Detach(entity);
    }

    private async Task _setStartProcessAsync(Topic topic, long taskId, long size, CancellationToken cancellationToken)
    {
        _database.Attach(topic);

        topic.ClearAsrTaskData();
        topic.AsrTaskId = taskId;
        topic.Media.Progress = 0;
        topic.Media.OriginalSize = size;
        topic.Media.AsrStatus = AsrMediaStatus.ASRProcessing;
        topic.Media.ASRProcessStart = DateTime.Now;
        topic.Media.ASRProcessEnd = null;
        topic.Media.Update = DateTime.Now;
        topic.Update = DateTime.Now;

        await _database.SaveChangesAsync(cancellationToken);
        _database.Detach(topic);
    }

    private async Task _setErrorAsync(Topic topic, string error, CancellationToken cancellationToken)
    {
        _database.Attach(topic);
        topic.Media.AsrStatus = AsrMediaStatus.ASRFailed;
        topic.Media.Error = error;
        topic.Media.ASRProcessEnd = DateTime.Now;
        topic.Media.Update = DateTime.Now;

        await _database.SaveChangesAsync(cancellationToken);
        _database.Detach(topic);

        throw new Exception(error);
    }
}
