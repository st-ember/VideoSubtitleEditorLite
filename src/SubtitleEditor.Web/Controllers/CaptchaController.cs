using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Infrastructure.Models;
using SubtitleEditor.Web.Infrastructure.Services;

namespace SubtitleEditor.Web.Controllers;

[AllowAnonymous]
public class CaptchaController : Controller
{
    private readonly HttpContext? _httpContext;
    private readonly ICaptchaService _captchaService;

    public CaptchaController(
        IHttpContextAccessor httpContextAccessor,
        ICaptchaService captchaService
        )
    {
        _httpContext = httpContextAccessor.HttpContext;
        _captchaService = captchaService;
    }

    [HttpGet]
    public JsonResult GetImage()
    {
        var width = 109;
        var height = 38;

        var captchaCode = _captchaService.GenerateCaptchaCode();
        var result = _captchaService.GenerateCaptchaImage(width, height, captchaCode);

        if (_httpContext != null && result.CaptchaCode != null && result.CaptchaByteData != null)
        {
            _httpContext.Session.SetString("CaptchaCode", result.CaptchaCode);
            return Json(new WebResponse<string>(true, Convert.ToBase64String(result.CaptchaByteData)));
        }

        return Json(new WebResponse<string>() { Success = false });
    }
}
