using static SubtitleEditor.Infrastructure.Models.Asr.NctuTaskSubtitleLinkResponse;

namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuTaskSubtitleLinkResponse : NctuResponseBase<NctuTaskSubtitleLinkResponseData>
{
    public Dictionary<string, string> Urls => Data
        .GroupBy(o => o.Type)
        .Select(o => o.First())
        .ToDictionary(o => o.Type, o => o.Url);

    public class NctuTaskSubtitleLinkResponseData
    {
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty; // SRT, TXT
        public string Url { get; set; } = string.Empty;
    }
}