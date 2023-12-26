using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Infrastructure.Models.SystemOption;

namespace SubtitleEditor.Infrastructure.Services;

public interface ISystemOptionService
{
    Task<SystemOptionModel?> GetAsync(string name);
    Task<string?> GetContentAsync(string name);
    Task<int?> GetIntAsync(string name);
    Task<long?> GetLongAsync(string name);
    Task<bool?> GetBooleanAsync(string name);
    Task<SystemOptionContext> ListAsync(params string[] names);
    Task<SystemOptionContext> ListAsync(IEnumerable<string> names);
    Task<SystemOptionContext> ListVisibledAsync(params string[] names);
    Task<SystemOptionContext> ListVisibledAsync(IEnumerable<string> names);
    Task SetAsync(ISystemOption systemOptionModel);
    Task RestoreToDefaultAsync(string name);
    Task InitializeSystemOptionsAsync(IEnumerable<SystemOptionModel> systemOptions);
}
