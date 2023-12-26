namespace SubtitleEditor.Infrastructure.Models.User;

public class UserModifyRecordData
{
    public string? Time { get; set; }
    public string? Action { get; set; }
    public UserModifyRecordChangeData ChangeData { get; set; } = new();
    public string? Editor { get; set; }
}
