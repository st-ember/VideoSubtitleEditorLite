namespace SubtitleEditor.Infrastructure.Models.User;

public class UserModifyRecordChangeItem
{
    public string Field { get; set; } = string.Empty;
    public string? Before { get; set; }
    public string? After { get; set; }

    public static UserModifyRecordChangeItem From(string field, string? before, string? after)
    {
        return new UserModifyRecordChangeItem
        {
            Field = field,
            Before = before,
            After = after
        };
    }
}