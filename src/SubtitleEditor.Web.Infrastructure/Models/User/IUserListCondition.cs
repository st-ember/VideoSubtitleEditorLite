namespace SubtitleEditor.Web.Infrastructure.Models.User;

public interface IUserListCondition
{
    string Keyword { get; set; }
    string Status { get; set; }
}