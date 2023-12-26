using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.ActionFilters;

namespace SubtitleEditor.Web.Controllers;

public class PermissionController : AuthorizedController
{
    private readonly IPermissionService _permissionService;

    public PermissionController(
        IPermissionService permissionService
        )
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    [ActionWithLog(SystemAction.GetSelfPermissionData, OnlyLogOnError = true)]
    public async Task<IActionResult> Get()
    {
        var result = await _permissionService.GetLoginUserPermissionAsync();
        return Ok(new
        {
            result.IsSystemAdmin,
            Actions = result.Actions.Select(o => o.ToString())
        });
    }
}
