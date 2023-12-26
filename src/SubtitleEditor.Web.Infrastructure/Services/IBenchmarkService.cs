using SubtitleEditor.Web.Infrastructure.Models.Benchmark;

namespace SubtitleEditor.Web.Infrastructure.Services;

public interface IBenchmarkService
{
    Task<BenchmarkModel> DoBenchmarkAsync(Guid topicId, string? argumentTemplate = null);
}
