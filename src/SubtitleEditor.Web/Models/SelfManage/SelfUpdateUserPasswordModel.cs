namespace SubtitleEditor.Web.Models.SelfManage;

public class SelfUpdateUserPasswordModel
{
    public string Password { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string Confirm { get; set; } = string.Empty;
}