using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.ActionFilters;
using SubtitleEditor.Web.Infrastructure.Models.UserMeta;
using SubtitleEditor.Web.Models.SelfManage;

namespace SubtitleEditor.Web.Controllers;

public class UserMetaController : AuthorizedController
{
    private readonly IAccountService _accountService;
    private readonly IUserMetaService _userMetaService;
    private readonly ILogService _logService;

    public UserMetaController(
        IAccountService accountService,
        IUserMetaService userMetaService,
        ILogService logService
        )
    {
        _accountService = accountService;
        _userMetaService = userMetaService;
        _logService = logService;
    }

    [HttpGet]
    [ActionWithLog(SystemAction.GetSelfKeybinding, OnlyLogOnError = true)]
    public async Task<IActionResult> GetKeybinding()
    {
        if (!_accountService.TryGetLoginUserId(out var userId))
        {
            return NotFound();
        }

        _logService.Target = userId.ToString();

        var result = await _userMetaService.GetUserMetaDataAsync<UserKeybindingSet>(userId, UserMetaNames.Keybinding);
        if (result == null)
        {
            result = UserKeybindingSet.CreateDefault();
        }
        else
        {
            result.Ensure();
        }

        return Ok(result);
    }

    [HttpPost]
    [ActionWithLog(SystemAction.SaveSelfKeybinding)]
    public async Task<IActionResult> SaveKeybinding([FromBody] SelfUpdateKeybindingModel model)
    {
        if (!_accountService.TryGetLoginUserId(out var userId))
        {
            return NotFound();
        }

        _logService.Target = userId.ToString();

        await _userMetaService.CreateOrUpdateUserMetaAsync(userId, UserMetaNames.Keybinding, UserKeybindingSet.From(model.Keybindings));

        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.RecoverSelfKeybinding)]
    public async Task<IActionResult> RecoverKeybinding()
    {
        if (!_accountService.TryGetLoginUserId(out var userId))
        {
            return NotFound();
        }

        _logService.Target = userId.ToString();

        await _userMetaService.CreateOrUpdateUserMetaAsync(userId, UserMetaNames.Keybinding, UserKeybindingSet.CreateDefault());

        return Ok();
    }

    [HttpGet]
    [ActionWithLog(SystemAction.GetSelfOptions, OnlyLogOnError = true)]
    public async Task<IActionResult> GetSelfOptions()
    {
        if (!_accountService.TryGetLoginUserId(out var userId))
        {
            return NotFound();
        }

        _logService.Target = userId.ToString();

        return Ok(new SelfUpdateOptionsModel
        {
            FrameRate = await _userMetaService.GetUserMetaDataAsync<double?>(userId, UserMetaNames.FrameRate),
            WordLimit = await _userMetaService.GetUserMetaDataAsync<int?>(userId, UserMetaNames.WordLimit)
        });
    }

    [HttpPost]
    [ActionWithLog(SystemAction.SaveSelfOptions)]
    public async Task<IActionResult> SaveSelfOptions([FromBody] SelfUpdateOptionsModel model)
    {
        if (!_accountService.TryGetLoginUserId(out var userId))
        {
            return NotFound();
        }

        _logService.Target = userId.ToString();

        await _userMetaService.CreateOrUpdateUserMetaAsync(userId, UserMetaNames.FrameRate, model.FrameRate);
        await _userMetaService.CreateOrUpdateUserMetaAsync(userId, UserMetaNames.WordLimit, model.WordLimit);

        return Ok();
    }
}
