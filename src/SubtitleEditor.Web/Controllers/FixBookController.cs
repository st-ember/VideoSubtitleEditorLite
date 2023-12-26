using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Models.Asr;
using SubtitleEditor.Infrastructure.Models.FixBook;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.ActionFilters;
using SubtitleEditor.Web.Models.FixBook;

namespace SubtitleEditor.Web.Controllers;

public class FixBookController : AuthorizedController
{
    private readonly IFixBookService _fixBookService;
    private readonly ICacheService _cacheService;
    private readonly ILogService _logService;
    private readonly IActivationService _activationService;
    private readonly ISystemOptionService _systemOptionService;

    private const string _pageCacheKey = "fixBook_pages";

    public FixBookController(
        IFixBookService fixBookService,
        ICacheService cacheService,
        ILogService logService,
        IActivationService activationService,
        ISystemOptionService systemOptionService
        )
    {
        _fixBookService = fixBookService;
        _cacheService = cacheService;
        _logService = logService;
        _activationService = activationService;
        _systemOptionService = systemOptionService;
    }

    [HttpGet]
    [ViewWithLog(SystemAction.ShowFixBookView, OnlyLogOnError = true)]
    public async Task<IActionResult> List(FixBookListViewModel model)
    {
        var pages = Array.Empty<FixBookPageModel>();
        if (!_cacheService.ContainsKey(_pageCacheKey))
        {
            var fixBooks = await _fixBookService.ListAsync();
            pages = fixBooks.Any() ?
                fixBooks
                    .Select(m => new FixBookPageModel
                    {
                        ModelName = m.ModelName ?? string.Empty,
                        MaxFixbookSize = m.MaxFixbookSize
                    })
                    .ToArray() :
                new FixBookPageModel[]
                {
                    new FixBookPageModel
                    {
                        ModelName = "Default"
                    }
                };

            _cacheService.Set(_pageCacheKey, TimeSpan.FromHours(8), pages);
        }
        else
        {
            pages = _cacheService.Get<FixBookPageModel[]>(_pageCacheKey)!;
        }

        if (string.IsNullOrWhiteSpace(model.ModelName) || !pages.Any(m => m.ModelName == model.ModelName))
        {
            model.ModelName = pages.First().ModelName;
        }

        model.Pages = pages;

        var key = await _systemOptionService.GetContentAsync(SystemOptionNames.ActivationKey);
        var activationData = _activationService.ResolveKey(key);
        var asrAccess = _activationService.CheckAsrAccess(activationData);

        if (asrAccess.Message == "Full")
        {
            model.AsrAccess = true;
        }
        else if (asrAccess.Message == "Limited")
        {
            model.AsrAccess = false;
        }
        else
        {
            model.AsrAccess = false;
        }

        return View(model);
    }

    [HttpGet]
    [ActionWithLog(SystemAction.GetFixBook, OnlyLogOnError = true)]
    public async Task<IActionResult> Get(string modelName)
    {
        var fixBookData = await _fixBookService.GetByModelAsync(modelName);
        return Ok(fixBookData);
    }

    [HttpPost]
    [ActionWithLog(SystemAction.SaveFixBook)]
    public async Task<IActionResult> Save([FromBody] FixBookSaveRequest model)
    {
        await _fixBookService.SaveAsync(new FixBookData
        {
            ModelName = model.ModelName,
            Items = model.FixBookItems
        });

        return Ok();
    }
}
