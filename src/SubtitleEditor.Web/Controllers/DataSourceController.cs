using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.ActionFilters;

namespace SubtitleEditor.Web.Controllers;

public class DataSourceController : AuthorizedController
{
    private readonly IAsrService _asrService;
    private readonly ICacheService _cacheService;

    private const string _asrModelCacheKey = "asr-model-cache";

    public DataSourceController(
        IAsrService asrService,
        ICacheService cacheService
        )
    {
        _asrService = asrService;
        _cacheService = cacheService;
    }

    [HttpGet]
    [ActionWithLog(SystemAction.ListAsrModel, OnlyLogOnError = true)]
    public async Task<IActionResult> ListAsrModelOption()
    {
        var models = await _cacheService.GetOrCreateAsync(_asrModelCacheKey, TimeSpan.FromMinutes(10), () => _asrService.ListModelAsync());
        return Ok(models.Select(m => new SelectOption(m.DisplayName, m.Name)).ToArray());
    }
}
