using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.Models.Editor;

namespace SubtitleEditor.Web.Controllers;

public class EditorController : AuthorizedController
{
    private readonly IPermissionService _permissionService;
    private readonly IActivationService _activationService;
    private readonly ISystemOptionService _systemOptionService;

    public EditorController(
        IPermissionService permissionService,
        IActivationService activationService,
        ISystemOptionService systemOptionService

        )
    {
        _permissionService = permissionService;
        _activationService = activationService;
        _systemOptionService = systemOptionService;
    }

    [HttpGet]
    public async Task<IActionResult> Subtitle(Guid id, EditorViewModel model)
    {
        if (id == default)
        {
            return RedirectToAction("Entry", "Home");
        }

        var permissionContext = await _permissionService.GetLoginUserPermissionAsync();
        if (permissionContext.ContainsAll(SystemAction.UpdateTopicSubtitle, SystemAction.UpdateTopicTranscript))
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

        return RedirectToAction("Entry", "Home");
    }
}
