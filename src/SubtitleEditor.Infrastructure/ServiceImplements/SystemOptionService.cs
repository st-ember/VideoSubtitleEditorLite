using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Models.SystemOption;
using SubtitleEditor.Infrastructure.Services;
using System.Collections.Immutable;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class SystemOptionService : ISystemOptionService
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogService _logService;
    private readonly IEncryptService _encryptService;
    private readonly ICacheService _cacheService;

    private const string _optionFileName = "options";
    private const string _cache_key = "system_option_cache_";
    private const string _cache_key_list = "system_option_cache_list";

    public SystemOptionService(
        IConfigurationService configurationService,
        ILogService logService,
        IEncryptService encryptService,
        ICacheService cacheService
        )
    {
        _configurationService = configurationService;
        _logService = logService;
        _encryptService = encryptService;
        _cacheService = cacheService;
    }

    public async Task<SystemOptionModel?> GetAsync(string name)
    {
        var context = await ListAsync(name);
        return context.FirstOrDefault();
    }

    public async Task<string?> GetContentAsync(string name)
    {
        var systemOption = await GetAsync(name);
        return systemOption?.Content;
    }

    public async Task<int?> GetIntAsync(string name)
    {
        var systemOption = await GetAsync(name);
        return systemOption?.ToInt();
    }

    public async Task<long?> GetLongAsync(string name)
    {
        var systemOption = await GetAsync(name);
        return systemOption?.ToLong();
    }

    public async Task<bool?> GetBooleanAsync(string name)
    {
        var systemOption = await GetAsync(name);
        return systemOption?.ToBoolean();
    }

    public Task<SystemOptionContext> ListAsync(params string[] names)
    {
        return _listAsync(names, visibled: null);
    }

    public Task<SystemOptionContext> ListAsync(IEnumerable<string> names)
    {
        return _listAsync(names, visibled: null);
    }

    public Task<SystemOptionContext> ListVisibledAsync(params string[] names)
    {
        return _listAsync(names, visibled: true);
    }

    public Task<SystemOptionContext> ListVisibledAsync(IEnumerable<string> names)
    {
        return _listAsync(names, visibled: true);
    }

    public async Task SetAsync(ISystemOption systemOptionModel)
    {
        _logService.Target = systemOptionModel.Name;

        var configuration = await _configurationService.GetConfigurationAsync(_optionFileName, new SystemOptionConfigurationModel());
        var entity = configuration.Options
            .Where(m => m.Name == systemOptionModel.Name)
            .FirstOrDefault() ??
            new SystemOption
            {
                Name = systemOptionModel.Name,
                Visible = !SystemOptionNames.InvisibleSystemOptionName.Contains(systemOptionModel.Name),
                Encrypted = SystemOptionNames.EncrypedSystemOptionName.Contains(systemOptionModel.Name)
            };

        if (entity.Encrypted && !string.IsNullOrWhiteSpace(entity.Content))
        {
            entity.Content = _encryptService.Decrypt(entity.Content);
        }

        if (entity.Content != systemOptionModel.Content)
        {
            _logService.SystemInfo(string.Empty, systemOptionModel.Name, nameof(entity.Content), entity.Encrypted ? "*" : entity.Content ?? "-", entity.Encrypted ? "*" : systemOptionModel.Content ?? "-");
            entity.Content = entity.Encrypted ? _encryptService.Encrypt(systemOptionModel.Content) : systemOptionModel.Content;
            await _configurationService.WriteConfigurationAsync(_optionFileName, configuration);
            _resetCache();
        }
    }

    public async Task RestoreToDefaultAsync(string name)
    {
        _logService.Target = name;

        var configuration = await _configurationService.GetConfigurationAsync(_optionFileName, new SystemOptionConfigurationModel());
        var defaultEntity = configuration.DefaultOptions
            .Where(m => m.Name == name)
            .FirstOrDefault();

        if (defaultEntity != null)
        {
            await SetAsync(defaultEntity);
        }
        else
        {
            throw new KeyNotFoundException($"找不到名為 {name} 的系統參數。");
        }
    }

    public async Task InitializeSystemOptionsAsync(IEnumerable<SystemOptionModel> systemOptions)
    {
        var configuration = await _configurationService.GetConfigurationAsync(_optionFileName, new SystemOptionConfigurationModel());
        var existNames = configuration.Options
            .Select(m => m.Name)
            .Distinct()
            .ToArray();

        var createdEntities = new List<SystemOption>();
        foreach (var name in SystemOptionNames.AllSystemOptionName.Where(o => !existNames.Contains(o)))
        {
            var systemOption = systemOptions.Any(o => o.Name == name) ? systemOptions.First(o => o.Name == name) : null;
            var entity = new SystemOption
            {
                Name = name,
                Content = systemOption?.Content ?? string.Empty,
                Description = systemOption?.Description ?? string.Empty,
                Visible = !SystemOptionNames.InvisibleSystemOptionName.Contains(name),
                Encrypted = SystemOptionNames.EncrypedSystemOptionName.Contains(name),
                Type = systemOption?.Type,
            };

            if (entity.Encrypted)
            {
                entity.Content = _encryptService.Encrypt(entity.Content);
            }

            createdEntities.Add(entity);
        }

        configuration.Options = configuration.Options.Concat(createdEntities).ToArray();
        configuration.DefaultOptions = configuration.Options.ToArray();
        await _configurationService.WriteConfigurationAsync(_optionFileName, configuration);
    }

    private async Task<SystemOptionContext> _listAsync(IEnumerable<string>? names, bool? visibled)
    {
        var key = $"{_cache_key}{(names != null ? string.Join(",", names) : string.Empty)}{(visibled.HasValue ? visibled.Value.ToString() : string.Empty)}";
        if (_cacheService.ContainsKey(key))
        {
            return _cacheService.Get<SystemOptionContext>(key)!;
        }

        var configuration = await _configurationService.GetConfigurationAsync(_optionFileName, new SystemOptionConfigurationModel());
        var array = configuration.Options
            .Where(m =>
                (names == null || !names.Any() || names.Contains(m.Name)) &&
                (!visibled.HasValue || visibled.Value == m.Visible)
            )
            .ToArray();

        foreach (var item in array.Where(e => e.Encrypted))
        {
            item.Content = _encryptService.Decrypt(item.Content);
        }

        var list = array
            .GroupBy(e => e.Name)
            .Select(g => g.OrderByDescending(e => e.Create).First())
            .Select(e => new { Index = Array.FindIndex(SystemOptionNames.AllSystemOptionName, o => o == e.Name), Model = new SystemOptionModel(e) })
            .OrderBy(o => o.Index)
            .Select(o => o.Model);

        var data = new SystemOptionContext(list);

        _cacheService.Set(key, data);
        _addKey(key);

        return data;
    }

    private void _addKey(string key)
    {
        var list = _cacheService.GetOrCreate(_cache_key_list, () => new List<string>());
        if (!list.Contains(key))
        {
            list.Add(key);
            _cacheService.Set(_cache_key_list, list.Distinct().ToList());
        }
    }

    private void _resetCache()
    {
        var list = _cacheService.GetOrCreate(_cache_key_list, () => new List<string>());
        if (list.Any())
        {
            foreach (var item in list)
            {
                _cacheService.Remove(item);
            }
            _cacheService.Set(_cache_key_list, new List<string>());
        }
    }
}