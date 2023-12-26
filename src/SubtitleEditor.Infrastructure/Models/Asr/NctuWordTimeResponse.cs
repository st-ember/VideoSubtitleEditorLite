namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuWordTimeResponse
{
    public NctuWordSegment[] Result { get; set; } = Array.Empty<NctuWordSegment>();
}