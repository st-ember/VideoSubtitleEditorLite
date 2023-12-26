namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuAsrServiceStatus
{
    public int TotalWorkers { get; set; }
    public int AvailableWorkers { get; set; }
    public int Status { get; set; }
    public string StatusDesc { get; set; } = string.Empty;
    public string FailureDesc { get; set; } = string.Empty;
    public string LicenseExpiredTime { get; set; } = string.Empty;
    public int CombinerStatus { get; set; }
    public int MaxActivatedUsers { get; set; }
    public string CombinerStatusDesc { get; set; } = string.Empty;
}