using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Models.User;

namespace SubtitleEditor.Infrastructure.Services;

public interface IUserService
{
    Task<bool> IsUserPasswordExpiredAsync(Guid userId);

    Task<bool> IsExistAsync(string account);

    Task<bool> IsExistAsync(Guid id);

    ISimpleResult CheckUser(IUser user);

    Task<User> GetAsync(Guid id);

    Task<User[]> ListAsync(IEnumerable<Guid> ids);

    Task<UserLoginRecordData[]> ListLoginRecordAsync(Guid id, DateTime start, DateTime end);

    Task<User> CreateAsync(IUser data, string password, UserStatus status = UserStatus.Enabled);

    Task UpdateAsync(IUser data);

    Task UpdateUserStatusAsync(Guid id, UserStatus userStatus);

    Task UpdateUserGroupAsync(Guid id, IEnumerable<Guid> userGroupIds);

    Task<User> UpdatePasswordAsync(Guid id, string newPassword, string confirm);

    Task<User> UpdatePasswordAsync(Guid id, string password, string newPassword, string confirm);

    Task RemoveAsync(Guid id);

    Task<User> ResetPasswordAsync(Guid id, string newPassword);

    bool IsValidPassword(string password);

    bool IsValidEmail(string password);

    string GenerateNewPassword(int length);

    Task<bool> CheckPasswordAsync(Guid id, string newPassword, string confirm);
    Task<bool> CheckPasswordAsync(Guid id, string password, string newPassword, string confirm);

    Task EnsureDefaultUserAsync();
}