namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuResponseBase
{
    public int Code { get; set; }
    public string? Error { get; set; }
    public bool Success => Code == 200;
}

public class NctuResponseBase<TData> : NctuResponseBase
{
    public TData[] Data { get; set; } = Array.Empty<TData>();
}
