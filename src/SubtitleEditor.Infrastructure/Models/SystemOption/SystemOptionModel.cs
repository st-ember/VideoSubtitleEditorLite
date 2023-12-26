using SubtitleEditor.Core.Abstract;

namespace SubtitleEditor.Infrastructure.Models.SystemOption;

public class SystemOptionModel : ISystemOption
{
    public string Name { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Description { get; set; }
    public bool Encrypted { get; set; }
    public string? Type { get; set; }

    public SystemOptionModel() { }

    public SystemOptionModel(string name, string? content, string? description, string? type = null)
    {
        Name = name;
        Content = content;
        Description = description;
        Type = type;
    }

    public SystemOptionModel(Database.Entities.SystemOption entity)
    {
        Name = entity.Name;
        Content = entity.Content;
        Description = entity.Description;
        Encrypted = entity.Encrypted;
        Type = entity.Type;
    }

    public int? ToInt()
    {
        return !string.IsNullOrWhiteSpace(Content) && int.TryParse(Content, out var n) ? n : null;
    }

    public long? ToLong()
    {
        return !string.IsNullOrWhiteSpace(Content) && long.TryParse(Content, out var n) ? n : null;
    }

    public bool? ToBoolean()
    {
        return !string.IsNullOrWhiteSpace(Content) && bool.TryParse(Content, out var n) ? n : null;
    }
}
