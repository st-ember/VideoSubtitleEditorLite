namespace SubtitleEditor.Core.Attributes;

/// <summary>
/// 名稱與描述文字。
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class NameAttribute : Attribute
{
    public string? Name { get; }
    public string? Description { get; }

    public NameAttribute(string name)
    {
        Name = name;
    }

    public NameAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}