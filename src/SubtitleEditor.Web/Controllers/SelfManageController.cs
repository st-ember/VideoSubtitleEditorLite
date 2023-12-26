using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Helpers;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.ActionFilters;
using SubtitleEditor.Web.Models.SelfManage;

namespace SubtitleEditor.Web.Controllers;

public class SelfManageController : AuthorizedController
{
    private readonly IAccountService _accountService;
    private readonly IUserService _userService;
    private readonly IUserGroupService _userGroupService;

    public SelfManageController(
        IAccountService accountService, 
        IUserService userService,
        IUserGroupService userGroupService
        )
    {
        _accountService = accountService;
        _userService = userService;
        _userGroupService = userGroupService;
    }

    [HttpGet]
    [ActionWithLog(SystemAction.GetSelfModifyData, OnlyLogOnError = true)]
    public async Task<IActionResult> GetSelfModifyData()
    {
        var user = await _userService.GetAsync(_getLoginedUserId());
        return Ok(user.ToUserData());
    }

    [HttpGet]
    [ActionWithLog(SystemAction.GetSelfModifyGroupData, OnlyLogOnError = true)]
    public async Task<IActionResult> GetSelfModifyGroupData()
    {
        var entities = await _userGroupService.ListByUserAsync(_getLoginedUserId());
        return Ok(entities.Select(e => e.ToUserGroupData()));
    }

    [HttpPost]
    [ActionWithLog(SystemAction.SelfUpdateUser)]
    public async Task<IActionResult> SelfUpdateUser([FromBody] SelfUpdateUserModel model)
    {
        if (model == null)
        {
            throw new ArgumentException("輸入的格式錯誤！");
        }

        var userId = _getLoginedUserId();
        var originalData = await _userService.GetAsync(userId);
        model.Id = userId;
        model.Account = originalData.Account;
        model.Description = originalData.Description;
        model.Status = originalData.Status;

        if (!_userService.CheckUser(model).Success)
        {
            throw new ArgumentException("帳號資訊未填寫完整！");
        }

        await _userService.UpdateAsync(model);
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.SelfUpdateUserPassword)]
    public async Task<IActionResult> SelfUpdateUserPassword([FromBody] SelfUpdateUserPasswordModel model)
    {
        if (model == null)
        {
            throw new ArgumentException("輸入的格式錯誤！");
        }

        await _userService.UpdatePasswordAsync(_getLoginedUserId(), model.Password, model.NewPassword, model.Confirm);
        return Ok();
    }

    private Guid _getLoginedUserId()
    {
        return _accountService.TryGetLoginUserId(out var userId) ? userId : throw new UnauthorizedAccessException("無法取得已登入的帳號");
    }
}
