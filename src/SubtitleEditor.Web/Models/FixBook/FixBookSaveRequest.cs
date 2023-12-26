using SubtitleEditor.Infrastructure.Models.FixBook;

namespace SubtitleEditor.Web.Models.FixBook;

public class FixBookSaveRequest
{
    public string ModelName { get; set; } = string.Empty;
    public FixBookItem[] FixBookItems { get; set; } = Array.Empty<FixBookItem>();
}