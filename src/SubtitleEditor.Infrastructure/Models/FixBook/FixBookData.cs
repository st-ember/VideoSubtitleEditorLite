namespace SubtitleEditor.Infrastructure.Models.FixBook;

public class FixBookData
{
    public string? ModelName { get; set; }
    public int? MaxFixbookSize { get; set; }
    public FixBookItem[] Items { get; set; } = Array.Empty<FixBookItem>();
}
