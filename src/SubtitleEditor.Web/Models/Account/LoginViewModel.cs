namespace SubtitleEditor.Web.Models.Account;

public class LoginViewModel
{
    public string? Account { get; set; }
    public string? Password { get; set; }
    public string? CaptchaCode { get; set; }
    public string? Error { get; set; }
    public bool AsrAccess { get; set; } = false;
}
