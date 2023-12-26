namespace SubtitleEditor.Web.Models.Account;

public class RenewPasswordViewModel
{
    public Guid Id { get; set; }
    public string Password { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string Confirm { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
    public string? Error { get; set; }
}