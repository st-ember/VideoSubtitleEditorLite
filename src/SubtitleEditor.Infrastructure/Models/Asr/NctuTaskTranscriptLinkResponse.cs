using static SubtitleEditor.Infrastructure.Models.Asr.NctuTaskTranscriptLinkResponse;

namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuTaskTranscriptLinkResponse : NctuResponseBase<NctuTaskTranscriptLinkResponseData>
{
    public string Url => Data.Length > 0 ? Data[0].Url : string.Empty;

    public class NctuTaskTranscriptLinkResponseData
    {
        public long Id { get; set; }
        public string Owner { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}