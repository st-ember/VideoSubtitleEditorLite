using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Models.UserMeta;

namespace SubtitleEditor.Infrastructure.Helpers;

public static class UserMetaDataHelper
{
    public static UserMetaData ToUserMetaData(this UserMeta entity)
    {
        return new UserMetaData
        {
            UserId = entity.UserId,
            Key = entity.Key,
            Data = entity.Data
        };
    }

    public static void ApplyToEntity(this UserMetaData userData, UserMeta entity)
    {
        entity.Key = userData.Key;
        entity.Data = userData.Data;
    }
}