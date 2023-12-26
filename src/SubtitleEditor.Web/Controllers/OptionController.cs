using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Models.SystemOption;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.ActionFilters;
using SubtitleEditor.Web.Models.Options;

namespace SubtitleEditor.Web.Controllers;

public class OptionController : AuthorizedController
{
    private readonly ISystemOptionService _systemOptionService;
    private readonly ILogService _logService;
    private readonly IActivationService _activationService;

    public OptionController(
        ISystemOptionService systemOptionService,
        ILogService logService,
        IActivationService activationService
        )
    {
        _systemOptionService = systemOptionService;
        _logService = logService;
        _activationService = activationService;
    }

    [HttpGet]
    [ViewWithLog(SystemAction.ListOption, OnlyLogOnError = true)]
    public async Task<IActionResult> List(ListOptionModel model)
    {
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
    [ActionWithLog(SystemAction.ListOption, OnlyLogOnError = true)]
    public async Task<IActionResult> ListOption()
    {
        var context = await _systemOptionService.ListVisibledAsync(SystemOptionNames.AllSystemOptionName);
        var model = new ListOptionModel
        {
            Items = context
                .Select(m =>
                {
                    if (m.Value.Encrypted)
                    {
                        m.Value.Content = null;
                    }

                    return m.Value;
                })
                .ToArray()
        };

        return Ok(model);
    }

    [HttpPost]
    [ActionWithLog(SystemAction.UpdateOptions)]
    public async Task<IActionResult> Update([FromBody] UpdateOptionModel model)
    {
        var updatedNames = new List<string>();
        var context = await _systemOptionService.ListVisibledAsync(SystemOptionNames.AllSystemOptionName);

        foreach (var item in context)
        {
            var updatedItem = model.Items.Where(m => m.Name == item.Value.Name).FirstOrDefault();
            if (updatedItem == null)
            {
                continue;
            }

            if (!item.Value.Encrypted || item.Value.Encrypted && !string.IsNullOrEmpty(updatedItem.Content))
            {
                await _systemOptionService.SetAsync(new SystemOptionModel
                {
                    Name = item.Value.Name,
                    Description = item.Value.Description,
                    Content = updatedItem.Content,
                    Encrypted = item.Value.Encrypted
                });
            }
        }

        _logService.Target = null;

        return Ok();
    }
}
