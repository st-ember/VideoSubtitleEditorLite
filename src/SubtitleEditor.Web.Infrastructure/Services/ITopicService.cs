using SubtitleEditor.Core.Models;
using SubtitleEditor.Web.Infrastructure.Models.Topic;

namespace SubtitleEditor.Web.Infrastructure.Services;

public interface ITopicService
{
    ISimpleResult CheckForCreation(TopicCreationModel model);
    ISimpleResult CheckForUpdate(TopicUpdateModel model);
    Task<TopicPreviewData> GetAsync(Guid id);
    Task<TopicListData> GetTopicListItemAsync(Guid id);
    Task<TopicSubtitleData> GetSubtitleAsync(Guid id);
    Task<string?> GetTranscriptAsync(Guid id);
    Task<Stream?> GetRawFileAsync(Guid id);
    Task<long> GetTotalRawFileSizeAsync();
    Task<long> GetTotalStreamFileSizeAsync();
    Task CreateAsync(TopicCreationModel model);
    Task UpdateAsync(TopicUpdateModel model);
    Task UpdateWordLimitAsync(Guid id, int? wordLimit);
    Task ReplaceSubtitleAsync(Guid id, string ticket, double? frameRate);
    Task ReplaceTranscriptAsync(Guid id, string ticket);
    Task UpdateSubtitleAsync(TopicUpdateSubtitleModel model);
    Task UpdateTranscriptAsync(TopicUpdateTranscriptModel model);
    Task PauseAsync(IEnumerable<Guid> ids);
    Task ResumeAsync(IEnumerable<Guid> ids);
    Task ReExecuteAsync(IEnumerable<Guid> ids);
    Task SetNormalAsync(IEnumerable<Guid> ids);
    Task ArchiveAsync(IEnumerable<Guid> ids);
    Task RemoveAsync(IEnumerable<Guid> ids);
    Task RecoverToOriginalAsync(IEnumerable<Guid> ids);
    Task ReproduceSubtitleAsync(IEnumerable<Guid> ids);
    Task ReloadSubtitleAsync(IEnumerable<Guid> ids);
}
