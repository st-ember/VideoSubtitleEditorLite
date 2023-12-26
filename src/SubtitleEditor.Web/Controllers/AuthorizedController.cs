using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Infrastructure.Models;

namespace SubtitleEditor.Web.Controllers;

[Authorize]
public class AuthorizedController : Controller
{
    public JsonResult From(ILogResult logResult)
    {
        return new JsonResult(new WebResponse()
        {
            Success = logResult.Success,
            Message = logResult.Message
        });
    }

    public JsonResult From<TData>(ILogResult<TData> logResult)
    {
        return new JsonResult(new WebResponse<TData>()
        {
            Success = logResult.Success,
            Message = logResult.Message,
            Data = logResult.Data
        });
    }

    public JsonResult From(ISimpleResult logResult)
    {
        return new JsonResult(new WebResponse()
        {
            Success = logResult.Success,
            Message = logResult.Message
        });
    }

    public JsonResult From<TData>(ISimpleResult<TData> logResult)
    {
        return new JsonResult(new WebResponse<TData>()
        {
            Success = logResult.Success,
            Message = logResult.Message,
            Data = logResult.Data
        });
    }

    public JsonResult Ok(string message = "")
    {
        return new JsonResult(new WebResponse(true, message));
    }

    public JsonResult Ok<TData>(TData data, string message = "")
    {
        return new JsonResult(new WebResponse<TData>(true, data, message));
    }

    public void ThrowError(string message = "")
    {
        throw new Exception(message);
    }

    public void ThrowArgumentError(string message = "")
    {
        throw new ArgumentException(message);
    }

    public void ThrowAccessDeniedError(string message = "")
    {
        throw new AccessViolationException(message);
    }

    public void ThrowNotFound(string message = "")
    {
        throw new BadHttpRequestException(message);
    }
}
