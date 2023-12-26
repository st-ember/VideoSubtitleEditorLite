namespace SubtitleEditor.Infrastructure.Models.User;

public class UserLoginRecordData
{
    public Guid Id { get; set; }
    public bool Success { get; set; }
    public string? IPAddress { get; set; }
    public string? Message { get; set; }
    public string? Time { get; set; }
    public string? LogoutTime { get; set; }
}
