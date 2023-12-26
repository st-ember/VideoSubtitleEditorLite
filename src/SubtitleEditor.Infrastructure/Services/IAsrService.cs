using SubtitleEditor.Infrastructure.Models.Asr;

namespace SubtitleEditor.Infrastructure.Services;

public interface IAsrService
{
    Task<NctuTask?> GetTaskAsync(long id, CancellationToken? cancellationToken = null);
    Task<NctuTask[]> ListTaskAsync(NctuListTaskRequest request, CancellationToken? cancellationToken = null);
    Task<long> GetTaskTotalCountAsync(NctuListTaskRequest request, CancellationToken? cancellationToken = null);
    Task<string> GetTaskFileLinkAsync(long id, CancellationToken? cancellationToken = null);
    Task<Dictionary<string, string>> GetSubtitleLinkAsync(long id, CancellationToken? cancellationToken = null);
    Task<string> GetTaskTranscriptLinkAsync(long id, CancellationToken? cancellationToken = null);
    Task<NctuWordSegment[]> GetTaskWordSegmentsAsync(long id, CancellationToken? cancellationToken = null);
    Task<NctuASRModel[]> ListModelAsync(CancellationToken? cancellationToken = null);
    Task<Stream> RetrieveFileAsync(string url, CancellationToken? cancellationToken = null);
    Task<string> RetrieveTextFileAsync(string url, CancellationToken? cancellationToken = null);
    Task<long> CreateTaskAsync(NctuCreateTaskRequest request, CancellationToken? cancellationToken = null);
    Task DeleteTaskAsync(long id, CancellationToken? cancellationToken = null);
    Task<NctuFixBookModel[]> GetFixBookAsync(CancellationToken? cancellationToken = null);
    Task SaveFixBookAsync(NctuSaveFixBookRequest request, CancellationToken? cancellationToken = null);
    Task<NctuAsrVersion> GetAsrVersionAsync(CancellationToken? cancellationToken = null);
    Task<NctuAsrServiceStatus> GetAsrServiceStatusAsync(CancellationToken? cancellationToken = null);
}
