using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.ActionFilters;
using SubtitleEditor.Web.Infrastructure.ListAdeptors;
using SubtitleEditor.Web.Infrastructure.Models.Log;
using SubtitleEditor.Web.Models.Log;

namespace SubtitleEditor.Web.Controllers;

public class LogController : AuthorizedController
{
    private readonly IUserService _userService;
    private readonly IGenericListService<LogListProcessor> _listService;
    private readonly IActivationService _activationService;
    private readonly ISystemOptionService _systemOptionService;
    public LogController(
        IUserService userService,
        IGenericListService<LogListProcessor> listService,
        IActivationService activationService,
        ISystemOptionService systemOptionService
        )
    {
        _userService = userService;
        _listService = listService;
        _activationService = activationService;
        _systemOptionService = systemOptionService;
    }

    [HttpGet]
    [ViewWithLog(SystemAction.ListLog, OnlyLogOnError = true)]
    public async Task<IActionResult> List(LogListViewModel model)
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
        } else
        {
            model.AsrAccess = false;
        }

        return View(model);
    }

    [HttpGet]
    [ActionWithLog(SystemAction.ListLog, OnlyLogOnError = true)]
    public async Task<IActionResult> ListActionLog(LogListViewModel model)
    {
        await _listService.QueryAsync<LogListViewModel, LogListPrimaryData>(model);

        var userIds = model.List.Select(m => m.UserId).Where(o => o.HasValue).Select(o => o!.Value).ToArray();
        var users = await _userService.ListAsync(userIds);

        var response = LogListResponse.From(model);
        foreach (var item in response.List.Where(m => !string.IsNullOrWhiteSpace(m.UserId)))
        {
            var userId = !string.IsNullOrWhiteSpace(item.UserId) && Guid.TryParse(item.UserId, out Guid id) ? id : default;
            item.UserAccount = users.Where(e => e.Id == userId).SingleOrDefault()?.Account ?? "";
        }

        return Ok(response);
    }
}
