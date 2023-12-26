using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Web.ActionFilters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ViewWithLogAttribute : Attribute, IAsyncActionFilter
{
    public virtual bool OnlyLogOnError { get; set; }
    protected virtual string LogAction { get; set; }
    protected virtual SystemAction SystemAction { get; set; }

    public ViewWithLogAttribute(SystemAction logAction)
    {
        LogAction = logAction.ToString();
        SystemAction = logAction;
    }

    public virtual async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        var logService = context.HttpContext.RequestServices.GetRequiredService<ILogService>();
        logService.OnlyLogOnError = OnlyLogOnError;

        var httpRequest = context.HttpContext.Request;
        var logAction = !string.IsNullOrEmpty(LogAction) ? LogAction : httpRequest.Path.HasValue ? httpRequest.Path.Value : "";

        var permissionContext = await permissionService.GetLoginUserPermissionAsync();
        if (SystemAction.IsPermissionAction() && !permissionContext.IsSystemAdmin && !permissionContext.Contains(SystemAction))
        {
            context.Result = new RedirectToActionResult("Login", "Account", new { });
            return;
        }

        var model = context.ActionArguments.ContainsKey("model") ? context.ActionArguments["model"] : null;
        var actionResult = await logService.StartAsync(logAction, () =>
        {
            logService.Request = model;
            return next.Invoke();
        });

        if (model != null && model is ISimpleResult simpleResult)
        {
            simpleResult.Apply(actionResult);
        }

        if (!actionResult.Success && actionResult.Data != null)
        {
            var controller = (dynamic)context.Controller;
            controller.ViewData.Model = model;

            actionResult.Data.Result = new ViewResult()
            {
                TempData = controller.TempData,
                ViewData = controller.ViewData
            };
        }
    }
}