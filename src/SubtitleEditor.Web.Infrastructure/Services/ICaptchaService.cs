using SubtitleEditor.Web.Infrastructure.Models.Captcha;

namespace SubtitleEditor.Web.Infrastructure.Services;

public interface ICaptchaService
{
    string GenerateCaptchaCode();
    CaptchaResult GenerateCaptchaImage(int width, int height, string captchaCode);
}