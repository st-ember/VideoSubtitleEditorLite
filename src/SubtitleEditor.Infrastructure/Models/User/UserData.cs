using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;

namespace SubtitleEditor.Infrastructure.Models.User;

public class UserData : IUser
{
    public Guid Id { get; set; }

    /// <summary>
    /// 登入用的帳號
    /// </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 使用者職稱
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 使用者電話
    /// </summary>
    public string? Telephone { get; set; }

    /// <summary>
    /// 使用者信箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 使用者帳號描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 使用者狀態
    /// </summary>
    public UserStatus Status { get; set; }
}
