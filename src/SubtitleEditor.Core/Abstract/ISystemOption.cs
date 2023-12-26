namespace SubtitleEditor.Core.Abstract;

public interface ISystemOption
{
    string Name { get; set; }
    string? Content { get; set; }
    string? Description { get; set; }
}
