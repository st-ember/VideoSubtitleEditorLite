using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Attributes;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Infrastructure.Helpers;
using SubtitleEditor.Infrastructure.Models.UserGroup;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.ActionFilters;
using SubtitleEditor.Web.Infrastructure.ListAdeptors;
using SubtitleEditor.Web.Infrastructure.Models.UserGroup;
using SubtitleEditor.Web.Models.UserGroup;
using System.Reflection;

namespace SubtitleEditor.Web.Controllers;

public class UserGroupController : AuthorizedController
{
    private readonly ILogService _logService;
    private readonly IUserGroupService _userGroupService;
    private readonly IGenericListService<UserGroupListProcessor> _listService;
    private readonly IActivationService _activationService;
    private readonly ISystemOptionService _systemOptionService;

    public UserGroupController(
        ILogService logService,
        IUserGroupService userGroupService,
        IGenericListService<UserGroupListProcessor> listService,
        IActivationService activationService,
        ISystemOptionService systemOptionService
        )
    {
        _logService = logService;
        _userGroupService = userGroupService;
        _listService = listService;
        _activationService = activationService;
        _systemOptionService = systemOptionService;
    }

    [HttpGet]
    [ActionWithLog(SystemAction.ListUserGroup, OnlyLogOnError = true)]
    public async Task<IActionResult> ListAsOption()
    {
        var list = await _userGroupService.ListAsync();
        return Ok(data: list.Select(o => new SelectOption(o.Name, o.Id.ToString())));
    }

    [HttpGet]
    [ViewWithLog(SystemAction.ShowUserGroupListView, OnlyLogOnError = true)]
    public async Task<IActionResult> List(UserGroupListViewModel model)
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
    [ActionWithLog(SystemAction.ListUserGroup, OnlyLogOnError = true)]
    public async Task<IActionResult> ListUserGroup(UserGroupListViewModel model)
    {
        await _listService.QueryAsync<UserGroupListViewModel, UserGroupListData>(model);
        return Ok(UserGroupListResponse.From(model));
    }

    [HttpGet]
    [ActionWithLog(SystemAction.ListUserGroup, OnlyLogOnError = true)]
    public async Task<JsonResult> ListByUser(Guid id)
    {
        var entities = await _userGroupService.ListByUserAsync(id);
        return Ok(entities.Select(e => e.ToUserGroupData()));
    }

    [HttpGet]
    [ActionWithLog(SystemAction.GetUserGroup, OnlyLogOnError = true)]
    public async Task<JsonResult> Get(Guid id)
    {
        var entity = await _userGroupService.GetAsync(id);
        return Ok(entity.ToUserGroupData());
    }

    [HttpPost]
    [ActionWithLog(SystemAction.CreateUserGroup)]
    public async Task<JsonResult> Create([FromBody] UserGroupData model)
    {
        if (model == null)
        {
            throw new ArgumentException("輸入的格式錯誤！");
        }

        if (!_userGroupService.Check(model).Success)
        {
            throw new ArgumentException("資訊未填寫完整！");
        }

        await _userGroupService.CreateAsync(model);

        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.UpdateUserGroup)]
    public async Task<JsonResult> Update([FromBody] UserGroupData model)
    {
        if (model == null)
        {
            throw new ArgumentException("輸入的格式錯誤！");
        }

        _logService.Target = model.Id.ToString();

        if (!_userGroupService.Check(model).Success)
        {
            throw new ArgumentException("資訊未填寫完整！");
        }

        await _userGroupService.UpdateAsync(model);
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.DuplicateUserGroup)]
    public async Task<JsonResult> Duplicate([FromBody] DuplicateUserGroupModel model)
    {
        if (model == null)
        {
            throw new ArgumentException("輸入的格式錯誤！");
        }

        _logService.Target = model.Id.ToString();

        await _userGroupService.DuplicateAsync(model.Id);
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.DeleteUserGroup)]
    public async Task<JsonResult> Delete([FromBody] DeleteUserGroupModel model)
    {
        await _userGroupService.DeleteAsync(model.Id);
        return Ok();
    }

    [HttpGet]
    public JsonResult ListGroupTypeAsOption()
    {
        var list = EnumHelper.ListAllEnum<PermissionType>()
            .Select(o => new SelectOption(o.GetName(), o.ToString()))
            .ToArray();

        return Ok(list);
    }

    [HttpGet]
    public JsonResult ListPermissionAsOption()
    {
        var type = typeof(SystemAction);
        var actionList = EnumHelper.ListAllEnum<SystemAction>()
            .Select(action => new
            {
                action,
                attribute = type.GetRuntimeField(action.ToString())?.GetCustomAttribute<IsPermissionActionAttribute>()
            })
            .Where(o => o.attribute != null)
            .Select(o => new { o.action, attribute = o.attribute! });

        var options = new List<SelectOption>();
        var categoryOptions = new List<SelectOption>();
        foreach (var actionItem in actionList.Where(o => !o.attribute.PermissionTypes.Any()))
        {
            categoryOptions.Add(new SelectOption(actionItem.action.GetName(), actionItem.action.ToString()));
        }

        if (categoryOptions.Any())
        {
            options.Add(new SelectOption("基礎", "") { IsGroup = true, Children = categoryOptions.ToArray() });
        }

        var permissionTypeOptionMap = EnumHelper.ListAllEnum<PermissionType>()
            .ToDictionary(o => o, o => new List<SelectOption>());

        foreach (var actionItem in actionList.Where(o => o.attribute.PermissionTypes.Any()))
        {
            var permissionType = actionItem.attribute.PermissionTypes.First();
            permissionTypeOptionMap[permissionType].Add(new SelectOption(actionItem.action.GetName(), actionItem.action.ToString()));
        }

        foreach (var pair in permissionTypeOptionMap.Where(pair => pair.Value.Any()))
        {
            options.Add(new SelectOption(pair.Key.GetName(), pair.Key.ToString()) { IsGroup = true, Children = pair.Value.ToArray() });
        }

        return Ok(options);
    }
}
