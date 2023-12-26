using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Database;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Worker.Infrastructure.Services;

namespace SubtitleEditor.Worker.Infrastructure.ServiceImplements;

public class ClearTopicService : IClearTopicService
{
    private readonly EditorContext _database;
    private readonly ILogService _logService;
    private readonly IFileService _fileService;
    private readonly IAsrService _asrService;
    private readonly IStreamFileService _streamFileService;

    public ClearTopicService(
        IServiceProvider serviceProvider,
        ILogService logService,
        IFileService fileService,
        IAsrService asrService,
        IStreamFileService streamFileService
        )
    {
        _database = serviceProvider.GetDatabase();
        _logService = logService;
        _fileService = fileService;
        _asrService = asrService;
        _streamFileService = streamFileService;
    }

    public async Task ProcessNextAsync(CancellationToken cancellationToken)
    {
        var time = DateTime.Now.AddMonths(-1);
        var outdatedTopics = await _database.Topics
            .Where(e => e.Status == TopicStatus.Archived && e.Update < time)
            .ToArrayAsync(cancellationToken);

        var modified = false;
        foreach (var entity in outdatedTopics)
        {
            try
            {
                if (entity.AsrTaskId.HasValue)
                {
                    await _asrService.DeleteTaskAsync(entity.AsrTaskId.Value);
                }

                if (!string.IsNullOrEmpty(entity.Media.Ticket) && _fileService.ExistsInStorage(entity.Media.Ticket))
                {
                    await _fileService.DeleteFromStorageAsync(entity.Media.Ticket);
                }

                _streamFileService.DeleteFromStreamFolder(entity.Id.ToString());
            }
            catch (Exception ex)
            {
                _logService.SystemError($"刪除單集({entity.Id})檔案時發生錯誤：{ex}");
                continue;
            }

            AddInfo(entity.Name, nameof(entity.Status), entity.Status.GetDescription(), TopicStatus.Removed.GetDescription());
            entity.Status = TopicStatus.Removed;
            entity.Update = DateTime.Now;
        }

        if (modified)
        {
            await _database.SaveChangesAsync(cancellationToken);
        }

        _database.Detach(entities: outdatedTopics);
    }

    protected virtual void AddInfo(string target, string field, string before, string after)
    {
        _logService.SystemInfo("", target, field, before, after);
    }
}
