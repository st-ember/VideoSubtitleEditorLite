using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Web.Controllers;

public class ManagementController : AuthorizedController
{
    private readonly IPermissionService _permissionService;

    public ManagementController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> Entry()
    {
        var permissionContext = await _permissionService.GetLoginUserPermissionAsync();
        if (permissionContext.Contains(SystemAction.ListLog))
        {
            return RedirectToAction("List", "Log");
        }

        if (permissionContext.Contains(SystemAction.ShowUserListView))
        {
            return RedirectToAction("List", "User");
        }

        if (permissionContext.Contains(SystemAction.ShowUserGroupListView))
        {
            return RedirectToAction("List", "UserGroup");
        }

        if (permissionContext.Contains(SystemAction.ListOption))
        {
            return RedirectToAction("List", "Option");
        }

        if (permissionContext.Contains(SystemAction.ShowSystemStatusView))
        {
            return RedirectToAction("Index", "Status");
        }

        return RedirectToAction("Entry", "Home");
    }
}
