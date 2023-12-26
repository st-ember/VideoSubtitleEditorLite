using SubtitleEditor.Core.Contexts;

namespace SubtitleEditor.Core.Abstract;

public interface IUser
{
    Guid Id { get; set; }
    string Account { get; set; }
    string? Name { get; set; }
    string? Title { get; set; }
    string? Telephone { get; set; }
    string? Email { get; set; }
    string? Description { get; set; }
    UserStatus Status { get; set; }
}
