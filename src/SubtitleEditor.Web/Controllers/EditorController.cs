using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Web.Controllers;

public class EditorController : AuthorizedController
{
    private readonly IPermissionService _permissionService;

    public EditorController(
        IPermissionService permissionService
        )
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> Subtitle(Guid id)
    {
        if (id == default)
        {
            return RedirectToAction("Entry", "Home");
        }

        var permissionContext = await _permissionService.GetLoginUserPermissionAsync();
        if (permissionContext.ContainsAll(SystemAction.UpdateTopicSubtitle, SystemAction.UpdateTopicTranscript))
        {
            return View();
        }

        return RedirectToAction("Entry", "Home");
    }
}
