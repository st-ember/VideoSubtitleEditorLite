namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuCreateTaskRequest
{
    public string Filename { get; set; } = string.Empty;
    //public byte[] Data { get; set; } = Array.Empty<byte>();
    public Stream? Stream { get; set; }
    public string SourceType { get; set; } = "2";
    public string Title { get; set; } = $"VSE-{Guid.NewGuid()}";
    public string AudioChannel { get; set; } = "0";
    public string Description { get; set; } = "From Video Subtitle Editor";
    public string ModelName { get; set; } = string.Empty;
}
