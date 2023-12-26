using SubtitleEditor.Core.Contexts;

namespace SubtitleEditor.Core.Abstract;

public interface IUserGroup
{
    Guid Id { get; set; }
    string Name { get; set; }
    string Description { get; set; }
    PermissionType? GroupType { get; set; }
    string Permission { get; set; }
}
