namespace SubtitleEditor.Infrastructure.Services;

public interface IConfigurationService
{
    Task<TModel> GetConfigurationAsync<TModel>(string name, TModel defaultValue) where TModel : class;
    Task<TModel?> GetConfigurationAsync<TModel>(string name) where TModel : class;
    Task<string?> GetConfigurationAsync(string name);
    Task WriteConfigurationAsync<TModel>(string name, TModel configuration) where TModel : class;
}
