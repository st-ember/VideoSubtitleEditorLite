using static SubtitleEditor.Infrastructure.Models.Asr.NctuTaskFileLinkResponse;

namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuTaskFileLinkResponse : NctuResponseBase<NctuTaskFileLinkResponseData>
{
    public string Url => Data.Length > 0 ? Data[0].Url : string.Empty;

    public class NctuTaskFileLinkResponseData
    {
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
