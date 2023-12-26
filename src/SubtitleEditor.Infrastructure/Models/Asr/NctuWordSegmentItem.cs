using System.Text.Json.Serialization;

namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuWordSegmentItem
{
    public string Word { get; set; } = string.Empty;
    public string Length { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;

    [JsonIgnore]
    public double Second => double.Parse(Start);

    [JsonIgnore]
    public TimeSpan Time => TimeSpan.FromSeconds(Second);
}

