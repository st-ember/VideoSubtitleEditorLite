namespace SubtitleEditor.Web.Infrastructure.Models.Captcha;

public class CaptchaResult
{
    public string CaptchaCode { get; set; } = "";
    public byte[]? CaptchaByteData { get; set; }
    public string? CaptchBase64Data => CaptchaByteData != null ? Convert.ToBase64String(CaptchaByteData) : null;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}