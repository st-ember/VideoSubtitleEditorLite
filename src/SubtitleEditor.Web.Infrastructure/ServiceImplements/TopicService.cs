using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Database;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Helpers;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.Infrastructure.Models.Topic;
using SubtitleEditor.Web.Infrastructure.Services;
using System;

namespace SubtitleEditor.Web.Infrastructure.ServiceImplements;

public class TopicService : ITopicService
{
    private readonly EditorContext _database;
    private readonly IFileService _fileService;
    private readonly ISubtitleService _subtitleService;
    private readonly IFFMpegService _ffmpegService;
    private readonly ILogService _logService;
    private readonly IStreamFileService _streamFileService;
    private readonly IAsrService _asrService;
    private readonly ISystemOptionService _systemOptionService;
    private readonly IAccountService _accountService;
    private readonly IFixBookService _fixBookService;
    private readonly IActivationService _activationService;

    public TopicService(
        EditorContext database,
        IFileService fileService,
        ISubtitleService subtitleService,
        IFFMpegService ffmpegService,
        ILogService logService,
        IStreamFileService streamFileService,
        IAsrService asrService,
        ISystemOptionService systemOptionService,
        IAccountService accountService,
        IFixBookService fixBookService,
        IActivationService activationService
        )
    {
        _database = database;
        _fileService = fileService;
        _subtitleService = subtitleService;
        _ffmpegService = ffmpegService;
        _logService = logService;
        _streamFileService = streamFileService;
        _asrService = asrService;
        _systemOptionService = systemOptionService;
        _accountService = accountService;
        _fixBookService = fixBookService;
        _activationService = activationService;
    }

    public ISimpleResult CheckForCreation(TopicCreationModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return SimpleResult.IsFailed("名稱不可空白。");
        }

        if (string.IsNullOrWhiteSpace(model.Filename))
        {
            return SimpleResult.IsFailed("檔案名稱不可空白。");
        }

        if (string.IsNullOrWhiteSpace(model.Ticket))
        {
            return SimpleResult.IsFailed("需要上傳媒體檔案。");
        }
        else if (!_fileService.ExistsInCache(model.Ticket))
        {
            return SimpleResult.IsFailed("找不到上傳的檔案。");
        }

        if (model.CreateType == TopicCreateType.Subtitle && (string.IsNullOrWhiteSpace(model.SubtitleTicket) || !_fileService.ExistsInCache(model.SubtitleTicket)))
        {
            return SimpleResult.IsFailed("找不到上傳的字幕檔案。");
        }

        if (model.CreateType == TopicCreateType.Transcript && (string.IsNullOrWhiteSpace(model.TranscriptTicket) || !_fileService.ExistsInCache(model.TranscriptTicket)))
        {
            return SimpleResult.IsFailed("找不到上傳的逐字稿檔案。");
        }

        return SimpleResult.IsSuccess();
    }

    public ISimpleResult CheckForUpdate(TopicUpdateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return SimpleResult.IsFailed("名稱不可空白。");
        }

        return SimpleResult.IsSuccess();
    }

    public async Task<TopicPreviewData> GetAsync(Guid id)
    {
        _logService.Target = id.ToString();

        var entity = await _database.Topics
            .AsNoTracking()
            .Include(e => e.Media)
            .Include(e => e.Subtitle)
            .Where(e => e.Id == id && e.Status != TopicStatus.Removed)
            .SingleOrDefaultAsync() ?? throw new Exception("找不到指定的單集。");

        _logService.Target = entity.Name;

        var processTime = 0d;

        var asrTask = entity.GetAsrTaskData();
        if (asrTask != null && asrTask.ProcessTime.HasValue)
        {
            processTime += asrTask.ProcessTime.Value;
        }

        if (entity.Media.ProcessEnd.HasValue && entity.Media.ProcessStart.HasValue)
        {
            processTime += (entity.Media.ProcessEnd.Value - entity.Media.ProcessStart.Value).TotalSeconds;
        }

        return new TopicPreviewData
        {
            Name = entity.Name,
            Description = entity.Description,
            Filename = entity.Media.Filename,
            Extension = entity.Media.Extension,
            OriginalSize = entity.Media.OriginalSize,
            Size = entity.Media.Size,
            Length = entity.Media.Length,
            ProcessTime = processTime,
            FrameRate = entity.Media.FrameRate,
            WordLimit = entity.Subtitle?.WordLimit,
            Status = entity.Status,
            AsrTaskId = entity.AsrTaskId,
            ModelName = asrTask?.ModelName,
            AsrMediaStatus = entity.Media.AsrStatus,
            ConvertMediaStatus = entity.Media.ConvertStatus,
            Error = entity.Media.Error
        };
    }

    public async Task<TopicListData> GetTopicListItemAsync(Guid id)
    {
        _logService.Target = id.ToString();

        var entity = await _database.Topics
            .AsNoTracking()
            .Include(e => e.Media)
            .Where(e => e.Id == id && e.Status != TopicStatus.Removed)
            .SingleOrDefaultAsync() ?? throw new Exception("找不到指定的單集。");

        _logService.Target = entity.Name;

        var processTime = 0d;

        if (entity.Media.AsrStatus == AsrMediaStatus.ASRCompleted)
        {
            var asrTask = entity.GetAsrTaskData();
            if (asrTask != null && asrTask.ProcessTime.HasValue)
            {
                processTime += asrTask.ProcessTime.Value;
            }
        }

        if (entity.Media.ConvertStatus == ConvertMediaStatus.FFMpegCompleted)
        {
            if (entity.Media.ProcessEnd.HasValue && entity.Media.ProcessStart.HasValue)
            {
                processTime += (entity.Media.ProcessEnd.Value - entity.Media.ProcessStart.Value).TotalSeconds;
            }
        }

        var key = await _systemOptionService.GetContentAsync(SystemOptionNames.ActivationKey);
        var activationData = _activationService.ResolveKey(key);


        return new TopicListData
        {
            Id = entity.Id,
            Name = entity.Name,
            Extension = entity.Media.Extension,
            AsrTaskId = entity.AsrTaskId,
            OriginalSize = entity.Media.OriginalSize,
            Size = entity.Media.Size,
            Length = entity.Media.Length,
            LengthText = TimeSpan.FromSeconds(entity.Media.Length).ToString("hh\\:mm\\:ss"),
            ProcessTime = processTime,
            ProcessTimeText = processTime > 0 ? TimeSpan.FromSeconds(processTime).ToString("hh\\:mm\\:ss") : "-",
            TopicStatus = entity.Status,
            TopicStatusText = entity.Status.GetDescription(),
            AsrMediaStatus = entity.Media.AsrStatus,
            AsrMediaStatusText = entity.Media.AsrStatus.GetDescription(),
            ConvertMediaStatus = entity.Media.ConvertStatus,
            ConvertMediaStatusText = entity.Media.ConvertStatus.GetDescription(),
            MediaError = entity.Media.Error,
            Progress = entity.Media.Progress,
            CreatorId = entity.CreatorId,
            Update = entity.Update.ToString("yyyy/MM/dd HH:mm:ss"),
            Create = entity.Create.ToString("yyyy/MM/dd HH:mm:ss"),
            AsrAccess = _activationService.CheckAsrAccess(activationData).ToString(),
        };
    }

    public async Task<TopicSubtitleData> GetSubtitleAsync(Guid id)
    {
        _logService.Target = id.ToString();

        var topic = await _database.Topics
            .AsNoTracking()
            .Include(e => e.Media)
            .Include(e => e.Subtitle)
            .Where(e => e.Id == id && e.Status != TopicStatus.Removed)
            .SingleOrDefaultAsync() ?? throw new Exception("找不到指定的字幕資料。");

        if (topic.Subtitle == null)
        {
            throw new Exception("此單集沒有有效的字幕與逐字稿。");
        }

        var subtitle = topic.Subtitle.GetSubtitle();
        return new TopicSubtitleData
        {
            Name = topic.Name,
            Filename = topic.Media.Filename,
            Extension = topic.Media.Extension,
            Description = topic.Description,
            Subtitle = subtitle,
            Transcript = topic.Subtitle.Transcript,
            FrameRate = topic.Media.FrameRate,
            WordLimit = topic.Subtitle.WordLimit
        };
    }

    public async Task<string?> GetTranscriptAsync(Guid id)
    {
        _logService.Target = id.ToString();

        var topic = await _database.Topics
            .AsNoTracking()
            .Include(e => e.Subtitle)
            .Where(e => e.Id == id && e.Status != TopicStatus.Removed)
            .SingleOrDefaultAsync() ?? throw new Exception("找不到指定的逐字稿資料。");

        if (topic.Subtitle == null)
        {
            throw new Exception("此單集沒有有效的字幕與逐字稿。");
        }

        _logService.Target = topic.Name;

        return topic.Subtitle.Transcript;
    }

    public async Task<Stream?> GetRawFileAsync(Guid id)
    {
        _logService.Target = id.ToString();

        var topic = await _database.Topics
            .AsNoTracking()
            .Include(e => e.Media)
            .Where(e => e.Id == id && e.Status != TopicStatus.Removed)
            .SingleOrDefaultAsync() ?? throw new Exception("找不到指定的媒體資料。");

        _logService.Target = topic.Name;

        if (topic.AsrTaskId.HasValue)
        {
            var link = await _asrService.GetTaskFileLinkAsync(topic.AsrTaskId.Value);
            if (string.IsNullOrWhiteSpace(link))
            {
                throw new Exception("無法取得遠端連結。");
            }

            return await _asrService.RetrieveFileAsync(link);
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

            return await _fileService.ReadFromStorageAsync(topic.Media.Ticket);
        }
    }

    public async Task<long> GetTotalRawFileSizeAsync()
    {
        return await _database.Topics
            .AsNoTracking()
            .Include(e => e.Media)
            .Where(e => e.Status != TopicStatus.Removed)
            .SumAsync(e => e.Media.OriginalSize);
    }

    public Task<long> GetTotalStreamFileSizeAsync()
    {
        return Task.Run(() => _fileService.GetTotalLengthOfStreamStorage());
    }

    public async Task CreateAsync(TopicCreationModel model)
    {
        _logService.Target = model.Name;

        if (!_accountService.TryGetLoginUserId(out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        if (!_fileService.ExistsInCache(model.Ticket))
        {
            throw new Exception("找不到上傳的檔案或檔案無效。");
        }

        var rawFileLimit = await _systemOptionService.GetLongAsync(SystemOptionNames.RawFileStorageLimit) ?? 0;
        var mediaFileLength = await _fileService.ReadLengthFromCacheAsync(model.Ticket);
        if (rawFileLimit > 0 && await GetTotalRawFileSizeAsync() + mediaFileLength > rawFileLimit)
        {
            throw new Exception("檔案的儲存空間已用鑿。");
        }

        var streamFileLimit = await _systemOptionService.GetLongAsync(SystemOptionNames.StreamFileStorageLimit) ?? 0;
        if (streamFileLimit > 0 && await GetTotalStreamFileSizeAsync() >= streamFileLimit)
        {
            throw new Exception("串流檔案的儲存空間已用鑿。");
        }

        byte[] subtitleFile = Array.Empty<byte>();
        if (model.CreateType == TopicCreateType.Subtitle)
        {
            var file = await _fileService.RetrieveFromCacheAsync(model.SubtitleTicket ?? "");
            if (file == null || file.Length == 0)
            {
                throw new Exception("找不到上傳的字幕檔案或檔案無效。");
            }

            subtitleFile = file;
            _fileService.DeleteCache(model.SubtitleTicket!);
        }

        byte[] transcriptFile = Array.Empty<byte>();
        if (model.CreateType == TopicCreateType.Transcript)
        {
            var file = await _fileService.RetrieveFromCacheAsync(model.TranscriptTicket ?? "");
            if (file == null || file.Length == 0)
            {
                throw new Exception("找不到上傳的逐字稿檔案或檔案無效。");
            }

            transcriptFile = file;
            _fileService.DeleteCache(model.TranscriptTicket!);
        }

        var filename = model.Filename;
        var extension = filename.Split('.').Last();
        var ticket = await _fileService.TransferFileFromCacheToStorageAsync(model.Ticket, extension);
        var duration = await _ffmpegService.GetDurationAsync(_fileService.GetFilePathInStorage(ticket));

        _fileService.DeleteCache(model.Ticket);

        var topic = new Topic
        {
            Name = model.Name,
            Description = model.Description,
            CreatorId = userId,
            Media = new Media
            {
                Ticket = ticket,
                Filename = filename,
                Extension = extension,
                OriginalSize = mediaFileLength,
                Length = duration,
                FrameRate = model.FrameRate
            },
            Subtitle = new Database.Entities.Subtitle
            {
                WordLimit = model.WordLimit
            }
        };

        if (model.CreateType != TopicCreateType.ASR)
        {
            if (model.CreateType == TopicCreateType.Subtitle)
            {
                var subtitle = model.FrameRate.HasValue ?
                    await _subtitleService.ReadFileAsync(subtitleFile, model.FrameRate.Value) : await _subtitleService.ReadFileAsync(subtitleFile);

                topic.Subtitle.SetOrigianlSubtitle(subtitle);
                topic.Subtitle.SetSubtitle(subtitle);
                topic.Media.AsrStatus = AsrMediaStatus.ASRSkipped;
            }

            if (model.CreateType == TopicCreateType.Transcript)
            {
                var transcript = await _subtitleService.ReadFileToStringAsync(transcriptFile);
                topic.Subtitle.OriginalTranscript = transcript;
                topic.Subtitle.Transcript = transcript;
                topic.Media.AsrStatus = AsrMediaStatus.ASRSkipped;
            }
        }
        else
        {
            topic.SetModelName(model.ModelName);
        }

        _database.Add(topic);
        await _database.SaveChangesAsync();
        _database.Detach(topic);
    }

    public async Task UpdateAsync(TopicUpdateModel model)
    {
        _logService.Target = model.Id.ToString();

        var entity = await _database.Topics
            .Include(e => e.Media)
            .Include(e => e.Subtitle)
            .Where(e => e.Id == model.Id && e.Status != TopicStatus.Removed)
            .SingleAsync();

        _logService.Target = entity.Name;

        var modified = false;

        var adoptedDescription = model.Description?.Trim();
        if (entity.Description != adoptedDescription)
        {
            AddInfo(entity.Name, nameof(entity.Description), entity.Description ?? "-", adoptedDescription ?? "-");
            entity.Description = adoptedDescription;
            modified = true;
        }

        double? adoptedFrameRate = model.FrameRate.HasValue ? model.FrameRate.Value < 0 ? 1 : model.FrameRate.Value : null;
        if (entity.Media.FrameRate != adoptedFrameRate)
        {
            AddInfo(entity.Name, nameof(entity.Media.FrameRate), entity.Media.FrameRate?.ToString() ?? "-", adoptedFrameRate?.ToString() ?? "-");
            entity.Media.FrameRate = adoptedFrameRate;
            modified = true;
        }

        int? adoptedWordLimit = model.WordLimit.HasValue ? model.WordLimit.Value < 0 ? 1 : model.WordLimit.Value : null;
        if (entity.Subtitle != null && entity.Subtitle.WordLimit != adoptedWordLimit)
        {
            AddInfo(entity.Name, nameof(entity.Subtitle.WordLimit), entity.Subtitle.WordLimit?.ToString() ?? "-", adoptedWordLimit?.ToString() ?? "-");
            entity.Subtitle.WordLimit = adoptedWordLimit;
            modified = true;
        }

        var originalModleName = entity.GetModelName();
        var adoptedModelName = model.ModelName;
        if (originalModleName != adoptedModelName && entity.AsrTaskId.HasValue)
        {
            AddInfo(entity.Name, nameof(entity.Name), originalModleName, adoptedModelName ?? "-");
            entity.SetModelName(adoptedModelName);
            modified = true;
        }

        var adoptedName = model.Name.Trim();
        if (entity.Name != adoptedName)
        {
            AddInfo(entity.Name, nameof(entity.Name), entity.Name, adoptedName);
            entity.Name = adoptedName;
            modified = true;
        }

        if (modified)
        {
            entity.Update = DateTime.Now;
            await _database.SaveChangesAsync();
            _database.Detach(entity);
        }
    }

    public async Task UpdateWordLimitAsync(Guid id, int? wordLimit)
    {
        _logService.Target = id.ToString();

        var entity = await _database.Topics
            .Include(e => e.Subtitle)
            .Where(e => e.Id == id && e.Status != TopicStatus.Removed)
            .SingleAsync();

        _logService.Target = entity.Name;

        var modified = false;

        int? adoptedWordLimit = wordLimit.HasValue ? wordLimit.Value < 0 ? 1 : wordLimit.Value : null;
        if (entity.Subtitle != null && entity.Subtitle.WordLimit != adoptedWordLimit)
        {
            AddInfo(entity.Name, nameof(entity.Subtitle.WordLimit), entity.Subtitle.WordLimit?.ToString() ?? "-", adoptedWordLimit?.ToString() ?? "-");
            entity.Subtitle.WordLimit = adoptedWordLimit;
            modified = true;
        }

        if (modified)
        {
            entity.Update = DateTime.Now;
            await _database.SaveChangesAsync();
            _database.Detach(entity);
        }
    }

    public async Task ReplaceSubtitleAsync(Guid id, string ticket, double? frameRate)
    {
        _logService.Target = id.ToString();

        var topic = await _database.Topics
            .Include(e => e.Media)
            .Where(e => e.Id == id && e.Status != TopicStatus.Removed)
            .SingleOrDefaultAsync() ?? throw new Exception("找不到指定 Id 的單集。");

        var subtitleFile = await _fileService.RetrieveFromCacheAsync(ticket);
        if (subtitleFile == null || subtitleFile.Length == 0)
        {
            throw new Exception("找不到上傳的字幕檔案或檔案無效。");
        }

        _fileService.DeleteCache(ticket);

        var modified = false;
        var adoptedFrameRate = frameRate != null && frameRate <= 0 ? 1 : frameRate;
        if (topic.Media.FrameRate != adoptedFrameRate)
        {
            AddInfo(topic.Name, nameof(topic.Media.FrameRate), topic.Media.FrameRate?.ToString() ?? "-", adoptedFrameRate?.ToString() ?? "-");
            topic.Media.FrameRate = adoptedFrameRate;
            modified = true;
        }

        if (modified)
        {
            await _database.SaveChangesAsync();
        }

        _database.Detach(topic);

        var subtitle = topic.Media.FrameRate.HasValue ?
            await _subtitleService.ReadFileAsync(subtitleFile, topic.Media.FrameRate.Value) : await _subtitleService.ReadFileAsync(subtitleFile);

        await UpdateSubtitleAsync(new TopicUpdateSubtitleModel
        {
            Id = id,
            Lines = subtitle.Lines,
            ModifiedStates = subtitle.ModifiedStates
        });
    }

    public async Task ReplaceTranscriptAsync(Guid id, string ticket)
    {
        _logService.Target = id.ToString();

        if (!await _database.Topics.AnyAsync(e => e.Id == id && e.Status != TopicStatus.Removed))
        {
            throw new Exception("找不到指定 Id 的單集。");
        }

        var transcriptFile = await _fileService.RetrieveFromCacheAsync(ticket);
        if (transcriptFile == null || transcriptFile.Length == 0)
        {
            throw new Exception("找不到上傳的逐字稿檔案或檔案無效。");
        }

        _fileService.DeleteCache(ticket);

        var transcript = await _subtitleService.ReadFileToStringAsync(transcriptFile);
        await UpdateTranscriptAsync(new TopicUpdateTranscriptModel
        {
            Id = id,
            Transcript = transcript
        });
    }

    public async Task UpdateSubtitleAsync(TopicUpdateSubtitleModel model)
    {
        _logService.Target = model.Id.ToString();

        if (!await _database.Topics.AnyAsync(e => e.Id == model.Id && e.Status != TopicStatus.Removed))
        {
            throw new Exception("找不到指定 Id 的單集。");
        }

        var entity = await _database.Subtitles
            .Where(e => e.TopicId == model.Id)
            .SingleOrDefaultAsync();

        var creation = false;
        if (entity == null)
        {
            entity = new Database.Entities.Subtitle
            {
                TopicId = model.Id
            };

            _database.Subtitles.Add(entity);
            creation = true;
        }

        var subtitle = entity.GetSubtitle();
        subtitle.Lines = model.Lines;
        subtitle.ModifiedStates = model.ModifiedStates;
        subtitle.Srt = _subtitleService.ToSrt(subtitle);

        entity.SetSubtitle(subtitle);

        if (creation)
        {
            entity.SetOrigianlSubtitle(subtitle);
        }
        else
        {
            entity.Transcript = null;
        }

        entity.Update = DateTime.Now;
        await _database.SaveChangesAsync();
        _database.Detach(entity);
    }

    public async Task UpdateTranscriptAsync(TopicUpdateTranscriptModel model)
    {
        _logService.Target = model.Id.ToString();

        if (!await _database.Topics.AnyAsync(e => e.Id == model.Id && e.Status != TopicStatus.Removed))
        {
            throw new Exception("找不到指定 Id 的單集。");
        }

        var entity = await _database.Subtitles
            .Where(e => e.TopicId == model.Id)
            .SingleOrDefaultAsync();

        var creation = false;
        if (entity == null)
        {
            entity = new Database.Entities.Subtitle
            {
                TopicId = model.Id
            };

            _database.Subtitles.Add(entity);
            creation = true;
        }

        entity.Transcript = model.Transcript.Trim();

        if (creation)
        {
            entity.OriginalTranscript = model.Transcript.Trim();
        }
        else
        {
            entity.Data = string.Empty;
        }

        entity.Update = DateTime.Now;
        await _database.SaveChangesAsync();
        _database.Detach(entity);
    }

    public Task PauseAsync(IEnumerable<Guid> ids)
    {
        return _updateStatusAsync(ids, TopicStatus.Paused, TopicStatus.Archived);
    }

    public Task ResumeAsync(IEnumerable<Guid> ids)
    {
        return _updateStatusAsync(ids, TopicStatus.Normal, TopicStatus.Archived);
    }

    public async Task ReExecuteAsync(IEnumerable<Guid> ids)
    {
        _logService.Target = string.Join(", ", ids.Select(o => o.ToString()));

        var entities = await _database.Topics
            .Include(e => e.Subtitle)
            .Include(e => e.Media)
            .Where(e => ids.Contains(e.Id) && e.Status != TopicStatus.Removed)
            .ToArrayAsync();

        if (ids.Any(id => !entities.Any(e => e.Id == id)))
        {
            throw new Exception($"找不到指定的單集 ({string.Join(", ", ids.Where(id => !entities.Any(e => e.Id == id)).Select(o => o.ToString()))})，操作取消。");
        }

        _logService.Target = string.Join(", ", entities.Select(e => e.Name));

        var invalidEntities = entities.Where(e => e.Status == TopicStatus.Archived || e.Media.AsrStatus != AsrMediaStatus.ASRFailed && e.Media.ConvertStatus != ConvertMediaStatus.FFMpegFailed);
        if (invalidEntities.Any())
        {
            throw new Exception($"單集不在正確的狀態 ({string.Join(", ", invalidEntities.Select(e => e.Id.ToString()))})，操作取消。");
        }

        foreach (var entity in entities)
        {
            AddInfo(entity.Name, nameof(entity.Status), entity.Status.GetDescription(), TopicStatus.Normal.GetDescription());

            if (entity.Media.AsrStatus == AsrMediaStatus.ASRFailed)
            {
                entity.Media.AsrStatus = AsrMediaStatus.ASRWaiting;
            }

            if (entity.Media.ConvertStatus == ConvertMediaStatus.FFMpegFailed)
            {
                entity.Media.ConvertStatus = ConvertMediaStatus.FFMpegWaiting;
            }

            entity.Media.Error = null;
            entity.Media.Update = DateTime.Now;
            entity.Update = DateTime.Now;
        }

        await _database.SaveChangesAsync();
        _database.Detach(entities: entities);
    }

    public Task SetNormalAsync(IEnumerable<Guid> ids)
    {
        return _updateStatusAsync(ids, TopicStatus.Normal, null);
    }

    public Task ArchiveAsync(IEnumerable<Guid> ids)
    {
        return _updateStatusAsync(ids, TopicStatus.Archived, null);
    }

    public async Task RemoveAsync(IEnumerable<Guid> ids)
    {
        _logService.Target = string.Join(", ", ids.Select(o => o.ToString()));

        var modified = false;
        var entities = await _database.Topics
            .Include(e => e.Media)
            .Where(e => ids.Contains(e.Id) && e.Status != TopicStatus.Removed)
            .ToArrayAsync();

        if (ids.Any(id => !entities.Any(e => e.Id == id)))
        {
            throw new Exception($"找不到指定的單集 ({string.Join(", ", ids.Where(id => !entities.Any(e => e.Id == id)).Select(o => o.ToString()))})，操作取消。");
        }

        _logService.Target = string.Join(", ", entities.Select(e => e.Name));

        foreach (var entity in entities.Where(e => e.Status != TopicStatus.Removed))
        {
            if (entity.AsrTaskId.HasValue)
            {
                try
                {
                    await _asrService.DeleteTaskAsync(entity.AsrTaskId.Value);
                }
                catch (Exception ex)
                {
                    _logService.SystemError($"刪除 ASR 任務 {entity.AsrTaskId.Value} 失敗。錯誤內容：{ex}");
                }
            }

            if (!string.IsNullOrEmpty(entity.Media.Ticket) && _fileService.ExistsInStorage(entity.Media.Ticket))
            {
                await _fileService.DeleteFromStorageAsync(entity.Media.Ticket);
            }

            _streamFileService.DeleteFromStreamFolder(entity.Id.ToString());

            AddInfo(entity.Name, nameof(entity.Status), entity.Status.GetDescription(), TopicStatus.Removed.GetDescription());
            entity.Status = TopicStatus.Removed;
            entity.Update = DateTime.Now;
            modified = true;
        }

        if (modified)
        {
            await _database.SaveChangesAsync();
        }

        _database.Detach(entities: entities);
    }

    public async Task RecoverToOriginalAsync(IEnumerable<Guid> ids)
    {
        _logService.Target = string.Join(", ", ids.Select(o => o.ToString()));

        var entities = await _database.Topics
            .Include(e => e.Subtitle)
            .Include(e => e.Media)
            .Where(e => ids.Contains(e.Id) && e.Status != TopicStatus.Removed)
            .ToArrayAsync();

        if (ids.Any(id => !entities.Any(e => e.Id == id)))
        {
            throw new Exception($"找不到指定的單集 ({string.Join(", ", ids.Where(id => !entities.Any(e => e.Id == id)).Select(o => o.ToString()))})，操作取消。");
        }

        _logService.Target = string.Join(", ", entities.Select(e => e.Name));

        var invalidEntities = entities.Where(e => e.Subtitle == null);
        if (invalidEntities.Any())
        {
            throw new Exception($"單集不在可以還原的狀態 ({string.Join(", ", invalidEntities.Select(e => e.Id.ToString()))})，操作取消。");
        }

        foreach (var entity in entities)
        {
            entity.Subtitle!.SetSubtitle(entity.Subtitle.GetOrigianlSubtitle());
            entity.Subtitle.Transcript = entity.Subtitle.OriginalTranscript;

            entity.Update = DateTime.Now;
            entity.Subtitle.Update = DateTime.Now;
        }

        await _database.SaveChangesAsync();
        _database.Detach(entities: entities);
    }

    public async Task ReproduceSubtitleAsync(IEnumerable<Guid> ids)
    {
        _logService.Target = string.Join(", ", ids.Select(o => o.ToString()));

        var entities = await _database.Topics
            .Include(e => e.Subtitle)
            .Include(e => e.Media)
            .Where(e => ids.Contains(e.Id) && e.Status != TopicStatus.Removed)
            .ToArrayAsync();

        if (ids.Any(id => !entities.Any(e => e.Id == id)))
        {
            throw new Exception($"找不到指定的單集 ({string.Join(", ", ids.Where(id => !entities.Any(e => e.Id == id)).Select(o => o.ToString()))})，操作取消。");
        }

        _logService.Target = string.Join(", ", entities.Select(e => e.Name));

        var fileTicketMap = new Dictionary<Guid, string>();
        foreach (var entity in entities.Where(e => e.AsrTaskId.HasValue))
        {
            var link = await _asrService.GetTaskFileLinkAsync(entity.AsrTaskId!.Value);
            if (string.IsNullOrWhiteSpace(link))
            {
                fileTicketMap.Add(entity.Id, string.Empty);
                continue;
            }

            if (fileTicketMap.Values.Any(o => string.IsNullOrEmpty(o)))
            {
                continue;
            }

            using var stream = await _asrService.RetrieveFileAsync(link);
            var fileTicket = await _fileService.SaveToStorageAsync(stream, entity.Media.Extension);
            fileTicketMap.Add(entity.Id, fileTicket);
        }

        var invalidEntities = entities.Where(e => fileTicketMap.ContainsKey(e.Id) && string.IsNullOrEmpty(fileTicketMap[e.Id]));
        if (invalidEntities.Any())
        {
            throw new Exception($"取得原始檔案連結失敗 ({string.Join(", ", invalidEntities.Select(e => e.Id.ToString()))})，操作取消。");
        }

        foreach (var entity in entities)
        {
            var fileTicket = fileTicketMap[entity.Id];

            if (entity.Media.Ticket != fileTicket)
            {
                AddInfo(entity.Name, nameof(entity.Media.Ticket), entity.Media.Ticket ?? "-", fileTicket ?? "-");
                entity.Media.Ticket = fileTicket;
            }

            if (entity.Status != TopicStatus.Normal)
            {
                AddInfo(entity.Name, nameof(entity.Status), entity.Status.GetDescription(), TopicStatus.Normal.GetDescription());
                entity.Status = TopicStatus.Normal;
            }

            AddInfo(entity.Name, nameof(entity.Media.AsrStatus), entity.Media.AsrStatus.GetDescription(), AsrMediaStatus.ASRWaiting.GetDescription());
            entity.Media.AsrStatus = AsrMediaStatus.ASRWaiting;

            AddInfo(entity.Name, nameof(entity.Media.AsrStatus), entity.Media.ConvertStatus.GetDescription(), ConvertMediaStatus.FFMpegWaiting.GetDescription());
            entity.Media.ConvertStatus = ConvertMediaStatus.FFMpegWaiting;

            entity.Update = DateTime.Now;
            entity.Media.Update = DateTime.Now;
        }

        await _database.SaveChangesAsync();
        _database.Detach(entities: entities);
    }

    public async Task ReloadSubtitleAsync(IEnumerable<Guid> ids)
    {
        _logService.Target = string.Join(", ", ids.Select(o => o.ToString()));

        var entities = await _database.Topics
            .Include(e => e.Subtitle)
            .Where(e => ids.Contains(e.Id) && e.Status != TopicStatus.Removed)
            .ToArrayAsync();

        if (ids.Any(id => !entities.Any(e => e.Id == id)))
        {
            throw new Exception($"找不到指定的單集 ({string.Join(", ", ids.Where(id => !entities.Any(e => e.Id == id)).Select(o => o.ToString()))})，操作取消。");
        }

        _logService.Target = string.Join(", ", entities.Select(e => e.Name));

        foreach (var entity in entities)
        {
            var taskId = entity.AsrTaskId!.Value;
            var subtitleLink = await _asrService.GetSubtitleLinkAsync(taskId);
            var subtitle = _subtitleService.ParseFromSrt(await _asrService.RetrieveTextFileAsync(subtitleLink["SRT"]));
            subtitle.Srt = _subtitleService.ToSrt(subtitle);

            var nctuWordSegments = await _asrService.GetTaskWordSegmentsAsync(taskId);

            _subtitleService.UpdateSubtitleWithWordSegments(subtitle, nctuWordSegments);

            var fixBookData = await _fixBookService.GetByModelAsync();
            _subtitleService.ReplaceInSubtitle(subtitle, fixBookData.Items.ToDictionary(m => m.Original, m => m.Correction));

            entity.Subtitle!.SetSubtitle(subtitle);
            entity.Subtitle!.SetOrigianlSubtitle(subtitle);
            entity.Subtitle!.Update = DateTime.Now;
        }

        await _database.SaveChangesAsync();
        _database.Detach(entities: entities);
    }

    protected virtual void AddInfo(string target, string field, string before, string after)
    {
        _logService.SystemInfo("", target, field, before, after);
    }

    private async Task _updateStatusAsync(IEnumerable<Guid> ids, TopicStatus topicStatus, TopicStatus? deniedStatus)
    {
        _logService.Target = string.Join(", ", ids.Select(o => o.ToString()));

        var modified = false;
        var entities = await _database.Topics
            .Where(e => ids.Contains(e.Id) && e.Status != TopicStatus.Removed)
            .ToArrayAsync();

        if (ids.Any(id => !entities.Any(e => e.Id == id)))
        {
            throw new Exception($"找不到指定的單集 ({string.Join(", ", ids.Where(id => !entities.Any(e => e.Id == id)).Select(o => o.ToString()))})，操作取消。");
        }

        _logService.Target = string.Join(", ", entities.Select(e => e.Name));

        if (deniedStatus.HasValue)
        {
            var invalidEntities = entities.Where(e => e.Status == deniedStatus.Value);
            if (invalidEntities.Any())
            {
                throw new Exception($"單集不在正確的狀態 ({string.Join(", ", invalidEntities.Select(e => e.Id.ToString()))})，操作取消。");
            }
        }

        foreach (var entity in entities.Where(e => e.Status != topicStatus))
        {
            AddInfo(entity.Name, nameof(entity.Status), entity.Status.GetDescription(), topicStatus.GetDescription());
            entity.Status = topicStatus;
            entity.Update = DateTime.Now;
            modified = true;
        }

        if (modified)
        {
            await _database.SaveChangesAsync();
        }

        _database.Detach(entities: entities);
    }
}