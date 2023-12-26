using static SubtitleEditor.Infrastructure.Models.Asr.NctuCountTaskResponse;

namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuCountTaskResponse : NctuResponseBase<NctuCountTaskResponseData>
{
    public int TotalCount => Data.Length > 0 ? Data[0].Total : 0;

    public class NctuCountTaskResponseData
    {
        public int Total { get; set; }
    }
}
