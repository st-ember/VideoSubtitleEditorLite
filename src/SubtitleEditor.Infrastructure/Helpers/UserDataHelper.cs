using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Models.User;

namespace SubtitleEditor.Infrastructure.Helpers;

public static class UserDataHelper
{
    public static UserData ToUserData(this User entity)
    {
        return new UserData
        {
            Id = entity.Id,
            Account = entity.Account,
            Name = entity.Name,
            Title = entity.Title,
            Telephone = entity.Telephone,
            Email = entity.Email,
            Description = entity.Description,
            Status = entity.Status
        };
    }
}
