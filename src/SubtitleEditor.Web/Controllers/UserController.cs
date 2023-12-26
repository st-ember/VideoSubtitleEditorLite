using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Helpers;
using SubtitleEditor.Infrastructure.Models.User;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.ActionFilters;
using SubtitleEditor.Web.Infrastructure.ListAdeptors;
using SubtitleEditor.Web.Infrastructure.Models.User;
using SubtitleEditor.Web.Models.User;

namespace SubtitleEditor.Web.Controllers;

public class UserController : AuthorizedController
{
    private readonly ILogService _logService;
    private readonly IUserService _userService;
    private readonly IPermissionService _permissionService;
    private readonly IGenericListService<UserListProcessor> _listService;
    private readonly IActivationService _activationService;
    private readonly ISystemOptionService _systemOptionService;

    public UserController(
        ILogService logService,
        IUserService userService,
        IPermissionService permissionService,
        IGenericListService<UserListProcessor> listService,
        IActivationService activationService,
        ISystemOptionService systemOptionService
        )
    {
        _logService = logService;
        _userService = userService;
        _permissionService = permissionService;
        _listService = listService;
        _activationService = activationService;
        _systemOptionService = systemOptionService;
    }

    [HttpGet]
    public async Task<IActionResult> Entry()
    {
        var permissionContext = await _permissionService.GetLoginUserPermissionAsync();
        if (permissionContext.Contains(SystemAction.ShowUserListView))
        {
            return RedirectToAction("List", "User");
        }
        else if (permissionContext.Contains(SystemAction.ShowUserGroupListView))
        {
            return RedirectToAction("List", "UserGroup");
        }
        else
        {
            return RedirectToAction("Entry", "Home");
        }
    }

    [HttpGet]
    [ViewWithLog(SystemAction.ShowUserListView, OnlyLogOnError = true)]
    public async Task<IActionResult> List(UserListViewModel model)
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
    [ActionWithLog(SystemAction.ListUser, OnlyLogOnError = true)]
    public async Task<IActionResult> ListUser(UserListViewModel model)
    {
        await _listService.QueryAsync<UserListViewModel, UserListData>(model);
        return Ok(UserListResponse.From(model));
    }

    [HttpGet]
    [ActionWithLog(SystemAction.GetUser, OnlyLogOnError = true)]
    public async Task<JsonResult> GetUser(Guid id)
    {
        var entity = await _userService.GetAsync(id);
        return Ok(entity.ToUserData());
    }

    [HttpGet]
    [ActionWithLog(SystemAction.IsAccountExist, OnlyLogOnError = true)]
    public async Task<JsonResult> IsAccountExist(string account)
    {
        var existed = await _userService.IsExistAsync(account);
        return Ok(existed);
    }

    //[HttpGet]
    //[ActionWithLog(SystemAction.ListLoginRecord, OnlyLogOnError = true)]
    //public async Task<JsonResult> ListLoginRecord(Guid id, string start, string end)
    //{
    //    var adoptedStart = DateTime.TryParse(start, out DateTime s) ? s : default;
    //    var adoptedEnd = DateTime.TryParse(end, out DateTime e) ? e : default;
    //    var records = await _userService.ListLoginRecordAsync(id, adoptedStart, adoptedEnd.AddDays(1));
    //    return Ok(records);
    //}

    [HttpPost]
    [ActionWithLog(SystemAction.CreateUser)]
    public async Task<JsonResult> CreateUser([FromBody] UserCreationModel model)
    {
        if (model == null)
        {
            throw new ArgumentException("輸入的格式錯誤！");
        }

        if (string.IsNullOrEmpty(model.Password))
        {
            throw new ArgumentException("密碼不可空白！");
        }

        if (model.Password != model.Confirm)
        {
            throw new ArgumentException("密碼與確認密碼不相等！");
        }

        if (!_userService.IsValidPassword(model.Password))
        {
            throw new ArgumentException("密碼安全性不足！至少需要八個字元，且包含大小寫英文、數字與特殊符號。");
        }

        if (!_userService.CheckUser(model).Success)
        {
            throw new ArgumentException("帳號資訊未填寫完整！");
        }

        var entity = await _userService.CreateAsync(model, model.Password, model.Status);

        return Ok(entity.Id);
    }

    [HttpPost]
    [ActionWithLog(SystemAction.UpdateUser)]
    public async Task<JsonResult> UpdateUser([FromBody] UserData model)
    {
        if (model == null)
        {
            throw new ArgumentException("輸入的格式錯誤！");
        }

        _logService.Target = model.Id.ToString();

        if (!_userService.CheckUser(model).Success)
        {
            throw new ArgumentException("帳號資訊未填寫完整！");
        }

        await _userService.UpdateAsync(model);
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.UpdateUserStatus)]
    public async Task<JsonResult> UpdateStatus([FromBody] UpdateUserStatusModel model)
    {
        await _userService.UpdateUserStatusAsync(model.Id, model.Status);
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.UpdatePassword)]
    public async Task<JsonResult> UpdatePassword([FromBody] UpdatePasswordModel model)
    {
        await _userService.UpdatePasswordAsync(model.Id, model.NewPassword, model.Confirm);
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.UpdateUsersUserGroup)]
    public async Task<JsonResult> UpdateUsersUserGroup([FromBody] UpdateUsersUserGroupModel model)
    {
        await _userService.UpdateUserGroupAsync(model.Id, model.UserGroups);
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.RemoveUser)]
    public async Task<JsonResult> Remove([FromBody] RemoveUserModel model)
    {
        await _userService.RemoveAsync(model.Id);
        return Ok();
    }
}
