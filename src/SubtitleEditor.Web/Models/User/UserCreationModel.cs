using SubtitleEditor.Infrastructure.Models.User;

namespace SubtitleEditor.Web.Models.User;

public class UserCreationModel : UserData
{
    public string? Password { get; set; }
    public string? Confirm { get; set; }
}