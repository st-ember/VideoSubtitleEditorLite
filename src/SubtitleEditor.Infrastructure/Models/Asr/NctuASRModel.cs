namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuASRModel
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int IsDefaultModel { get; set; } // 0, 1
    public int ModelStatus { get; set; } // 0, 1
    public bool Customized { get; set; }
}