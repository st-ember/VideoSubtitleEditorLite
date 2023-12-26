namespace SubtitleEditor.Web.Models.User;

public class UpdatePasswordModel
{
    public Guid Id { get; set; }
    public string NewPassword { get; set; } = string.Empty;
    public string Confirm { get; set; } = string.Empty;
}
