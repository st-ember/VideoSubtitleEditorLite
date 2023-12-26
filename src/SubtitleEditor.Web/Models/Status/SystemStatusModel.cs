namespace SubtitleEditor.Web.Models.Status;

public class SystemStatusModel
{
    public string? AsrKernelVersion { get; set; }
    public string? CaptionMakerVersion { get; set; } 
    public string VideoSubtitleEditorVersion { get; set; } = string.Empty;
    public int? TotalWorkers { get; set; }
    public int? AvailableWorkers { get; set; }
    public int? AsrStatus { get; set; }
    public string? LicenseExpiredTime { get; set; }

    public long StorageLimit { get; set; }
    public long StreamFileLimit { get; set; }
    public long StorageLength { get; set; }
    public long StreamFileLength { get; set; }

    public bool Activated { get; set; }
    public string ActivationKeyPublisher { get; set; } = string.Empty;
    public string ActivatedTarget { get; set; } = string.Empty;
    public uint? CalCount { get; set; }
    public string ActivationEnd { get; set; } = string.Empty;
    public bool AsrAccess { get; set; } = false;
}
