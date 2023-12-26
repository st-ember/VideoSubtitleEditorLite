using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Database.Entities;

namespace SubtitleEditor.Infrastructure.Services;

public interface IUserGroupService
{
    Task<UserGroup> GetAsync(Guid id);
    Task<UserGroup[]> ListAsync();
    Task<UserGroup[]> ListByUserAsync(Guid id);
    ISimpleResult Check(IUserGroup userGroup);
    Task CreateAsync(IUserGroup userGroup);
    Task UpdateAsync(IUserGroup userGroup);
    Task DuplicateAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task EnsureDefaultGroupAsync();
}
