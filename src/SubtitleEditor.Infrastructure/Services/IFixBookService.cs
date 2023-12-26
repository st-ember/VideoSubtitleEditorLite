using SubtitleEditor.Infrastructure.Models.FixBook;

namespace SubtitleEditor.Infrastructure.Services;

public interface IFixBookService
{
    Task<FixBookData[]> ListAsync();
    Task<FixBookData> GetByModelAsync();
    Task<FixBookData> GetByModelAsync(string modelName);
    Task SaveAsync(FixBookData data);
}
