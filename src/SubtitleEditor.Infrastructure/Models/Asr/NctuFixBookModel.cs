namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuFixBookModel
{
    public string ModelName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxFixbookSize { get; set; }
    public NctuFixBookItem[] Fixbook { get; set; } = Array.Empty<NctuFixBookItem>();
}
