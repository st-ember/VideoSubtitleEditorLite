using SubtitleEditor.Core.Contexts;

namespace SubtitleEditor.Web.Models.User;

public class UpdateUserStatusModel
{
    public Guid Id { get; set; }
    public UserStatus Status { get; set; }
}
