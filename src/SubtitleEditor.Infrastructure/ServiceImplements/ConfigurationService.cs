using SubtitleEditor.Infrastructure.Services;
using System.Text.Json;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class ConfigurationService : IConfigurationService
{
    private readonly IFileService _fileService;

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ConfigurationService(
        IFileService fileService
        )
    {
        _fileService = fileService;
    }

    public async Task<TModel> GetConfigurationAsync<TModel>(string name, TModel defaultValue) where TModel : class
    {
        var configuration = await GetConfigurationAsync<TModel>(name);
        return configuration ?? defaultValue;
    }

    public async Task<TModel?> GetConfigurationAsync<TModel>(string name) where TModel : class
    {
        var configuration = await GetConfigurationAsync(name);
        if (configuration == null)
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<TModel>(configuration, _serializerOptions);
        }
        catch
        {
            return default;
        }
    }

    public async Task<string?> GetConfigurationAsync(string name)
    {
        var path = Path.Combine(_fileService.ConfigurationFolder, $"{name}.json");
        if (!File.Exists(path))
        {
            return null;
        }

        return await File.ReadAllTextAsync(path);
    }

    public async Task WriteConfigurationAsync<TModel>(string name, TModel configuration) where TModel : class
    {
        try
        {
            var path = Path.Combine(_fileService.ConfigurationFolder, $"{name}.json");
            var text = JsonSerializer.Serialize(configuration, _serializerOptions);
            await File.WriteAllTextAsync(path, text);
        }
        catch { }
    }
}
