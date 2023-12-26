using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Web.ActionFilters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ActionWithLogAttribute : Attribute, IAsyncActionFilter
{
    public virtual bool OnlyLogOnError { get; set; }
    protected virtual string LogAction { get; set; }
    protected virtual SystemAction SystemAction { get; set; }

    public ActionWithLogAttribute(SystemAction logAction)
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

        object? request = null;
        if (httpRequest.HasJsonContentType())
        {
            httpRequest.Body.Seek(0, SeekOrigin.Begin);

            using var stream = new MemoryStream();
            await httpRequest.Body.CopyToAsync(stream);

            using var streamReader = new StreamReader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            request = await streamReader.ReadToEndAsync();
        }
        else
        {
            request = httpRequest.QueryString.Value;
        }

        var permissionContext = await permissionService.GetLoginUserPermissionAsync();
        if (SystemAction.IsPermissionAction() && !permissionContext.IsSystemAdmin && !permissionContext.Contains(SystemAction))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var actionResult = await logService.StartAsync(logAction, async () =>
        {
            logService.Request = request;
            return await next.Invoke();
        });

        if (!actionResult.Success && actionResult.Data != null)
        {
            actionResult.Data.Result = new JsonResult(new SimpleResult(actionResult.Success, actionResult.Message));
        }
    }
}