using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Models;

namespace SubtitleEditor.Infrastructure.Models;

public class WebResult
{
    public static JsonResult From(ILogResult logResult)
    {
        return new JsonResult(new WebResponse()
        {
            Success = logResult.Success,
            Message = logResult.Message,
            Code = logResult.Code
        });
    }

    public static JsonResult From<TData>(ILogResult<TData> logResult)
    {
        return new JsonResult(new WebResponse<TData>()
        {
            Success = logResult.Success,
            Message = logResult.Message,
            Code = logResult.Code,
            Data = logResult.Data
        });
    }

    public static JsonResult From(ISimpleResult logResult)
    {
        return new JsonResult(new WebResponse()
        {
            Success = logResult.Success,
            Message = logResult.Message
        });
    }

    public static JsonResult From<TData>(ISimpleResult<TData> logResult)
    {
        return new JsonResult(new WebResponse<TData>()
        {
            Success = logResult.Success,
            Message = logResult.Message,
            Data = logResult.Data
        });
    }

    public static JsonResult Ok(string message = "")
    {
        return new JsonResult(new WebResponse(true, message));
    }

    public static JsonResult Error(string message = "")
    {
        return new JsonResult(new WebResponse(false, message));
    }

    public static JsonResult Error(Exception exception)
    {
        return new JsonResult(new WebResponse(false, exception.ToString()));
    }

    public static JsonResult NotFound(string message = "not found")
    {
        return new JsonResult(new WebResponse(false, message));
    }

    public static JsonResult Ok<TData>(TData data, string message = "")
    {
        return new JsonResult(new WebResponse<TData>(true, data, message));
    }
}