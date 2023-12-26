namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuLoginResponse : NctuResponseBase
{
    public string Token { get; set; } = string.Empty;
    public int Expiration { get; set; }
}
