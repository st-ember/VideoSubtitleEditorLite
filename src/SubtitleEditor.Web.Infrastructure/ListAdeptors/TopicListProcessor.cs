using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Helpers;
using SubtitleEditor.Infrastructure.ListAdeptors;
using SubtitleEditor.Infrastructure.ListAdeptors.OrderMap;
using SubtitleEditor.Web.Infrastructure.ListAdeptors.OrderMap;
using SubtitleEditor.Web.Infrastructure.Models.Topic;

namespace SubtitleEditor.Web.Infrastructure.ListAdeptors;

public class TopicListProcessor : BasicListProcessor<Topic, ITopicListCondition>
{
    protected override OrderMap<Topic> OrderFuncMap => TopicListOrderMap.OrderFuncMap;

    protected override Task<IQueryable<Topic>> QueryEntityAsync()
    {
        var filterByKeyword = Condition != null && !string.IsNullOrWhiteSpace(Condition.Keyword);
        var adoptedKeyword = filterByKeyword ? Condition!.Keyword!.Trim().ToUpper() : "";

        var filterByTopicStatus = Condition != null && !string.IsNullOrWhiteSpace(Condition.TopicStatus) && Condition.TopicStatus != "-1";
        var adoptedTopicStatus = filterByTopicStatus ? 
            new TopicStatus[] { TopicStatus.Normal.Parse(Condition!.TopicStatus) } : 
            new TopicStatus[] { TopicStatus.Normal, TopicStatus.Paused };

        var filterByAsrMediaStatus = Condition != null && !string.IsNullOrWhiteSpace(Condition.AsrMediaStatus) && Condition.AsrMediaStatus != "-1";
        var adoptedAsrMediaStatus = filterByAsrMediaStatus ? AsrMediaStatus.ASRWaiting.Parse(Condition!.AsrMediaStatus) : AsrMediaStatus.ASRWaiting;

        var filterByConvertMediaStatus = Condition != null && !string.IsNullOrWhiteSpace(Condition.ConvertMediaStatus) && Condition.ConvertMediaStatus != "-1";
        var adoptedConvertMediaStatus = filterByConvertMediaStatus ? ConvertMediaStatus.FFMpegWaiting.Parse(Condition!.ConvertMediaStatus) : ConvertMediaStatus.FFMpegWaiting;

        var adoptedStart = Condition != null && DateTime.TryParse(Condition.Start, out DateTime start) ? start : DateTime.Today.AddDays(-7);
        var adoptedEnd = (Condition != null && DateTime.TryParse(Condition.End, out DateTime end) ? end : DateTime.Today).AddDays(1);
        if (adoptedStart > adoptedEnd)
        {
            adoptedStart = adoptedEnd.AddDays(-1);
        }

        return Task.FromResult(Database.Topics
            .AsNoTracking()
            .Include(e => e.Media)
            .Where(e =>
                e.Status != TopicStatus.Removed && adoptedTopicStatus.Contains(e.Status) &&
                adoptedStart <= e.Create && e.Create <= adoptedEnd &&
                (!filterByKeyword || EF.Functions.Like(e.Name, $"%{adoptedKeyword}%") || EF.Functions.Like(e.Media.Extension, $"%{adoptedKeyword}%")) &&
                (!filterByAsrMediaStatus || e.Media.AsrStatus == adoptedAsrMediaStatus) &&
                (!filterByConvertMediaStatus || e.Media.ConvertStatus == adoptedConvertMediaStatus)
                ));
    }

    protected override async Task<IWithId[]> RetrieveDataAsync()
    {
        var entities = await Database.Topics
            .AsNoTracking()
            .Include(e => e.Media)
            .Where(e => Ids.Contains(e.Id))
            .ToArrayAsync();

        var list = new List<TopicListData>();
        foreach (var e in entities)
        {
            var processTime = 0d;

            if (e.Media.AsrStatus == AsrMediaStatus.ASRCompleted)
            {
                var asrTask = e.GetAsrTaskData();
                if (asrTask != null && asrTask.ProcessTime.HasValue)
                {
                    processTime += asrTask.ProcessTime.Value;
                }
            }

            if (e.Media.ConvertStatus == ConvertMediaStatus.FFMpegCompleted)
            {
                if (e.Media.ProcessEnd.HasValue && e.Media.ProcessStart.HasValue)
                {
                    processTime += (e.Media.ProcessEnd.Value - e.Media.ProcessStart.Value).TotalSeconds;
                }
            }

            var model = new TopicListData
            {
                Id = e.Id,
                Name = e.Name,
                Extension = e.Media.Extension,
                AsrTaskId = e.AsrTaskId,
                OriginalSize = e.Media.OriginalSize,
                Size = e.Media.Size,
                Length = e.Media.Length,
                LengthText = TimeSpan.FromSeconds(e.Media.Length).ToString("hh\\:mm\\:ss"),
                ProcessTime = processTime,
                ProcessTimeText = processTime > 0 ? TimeSpan.FromSeconds(processTime).ToString("hh\\:mm\\:ss") : "-",
                TopicStatus = e.Status,
                TopicStatusText = e.Status.GetDescription(),
                AsrMediaStatus = e.Media.AsrStatus,
                AsrMediaStatusText = e.Media.AsrStatus.GetDescription(),
                ConvertMediaStatus = e.Media.ConvertStatus,
                ConvertMediaStatusText = e.Media.ConvertStatus.GetDescription(),
                MediaError = e.Media.Error,
                Progress = e.Media.Progress,
                CreatorId = e.CreatorId,
                Update = e.Update.ToString("yyyy/MM/dd HH:mm:ss"),
                Create = e.Create.ToString("yyyy/MM/dd HH:mm:ss")
            };

            list.Add(model);
        }

        return list.ToArray();
    }
}
