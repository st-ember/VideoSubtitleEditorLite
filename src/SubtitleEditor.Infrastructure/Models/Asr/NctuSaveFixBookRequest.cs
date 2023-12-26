namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuSaveFixBookRequest
{
    public string ModelName { get; set; } = string.Empty;
    public NctuFixBookItem[] Fixbook { get; set; } = Array.Empty<NctuFixBookItem>();
}