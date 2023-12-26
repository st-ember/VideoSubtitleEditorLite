namespace SubtitleEditor.Web.Models.User;

public class UpdateUsersUserGroupModel
{
    public Guid Id { get; set; }
    public Guid[] UserGroups { get; set; } = Array.Empty<Guid>();
}