using System.Text.Json.Serialization;

namespace SubtitleEditor.Core.Models;

public class Subtitle
{
    /// <summary>
    /// 字幕檔的檔頭。
    /// </summary>
    public string? Header { get; set; }

    public SubtitleLine[] Lines { get; set; } = Array.Empty<SubtitleLine>();
    public string Srt { get; set; } = string.Empty;

    public SubtitleModifiedState[] ModifiedStates { get; set; } = Array.Empty<SubtitleModifiedState>();
}

public class SubtitleLine
{
    /// <summary>
    /// 字幕的起始時間，格式為 hh:mm:ss.fff
    /// </summary>
    public string Start { get; set; } = string.Empty;

    /// <summary>
    /// 字幕的結束時間，格式為 hh:mm:ss.fff
    /// </summary>
    public string End { get; set; } = string.Empty;

    /// <summary>
    /// 字幕文字。
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 原始字幕資料。
    /// </summary>
    public string OriginalContent { get; set; } = string.Empty;

    /// <summary>
    /// Vtt 的格式文字。
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// 字幕的時間片段資料。
    /// </summary>
    public SubtitleLineWordSegment[]? WordSegments { get; set; }

    /// <summary>
    /// 原始字幕的時間片段資料。
    /// </summary>
    public SubtitleLineWordSegment[]? OriginalWordSegments { get; set; }

    [JsonIgnore]
    public TimeSpan Time => TimeSpan.Parse(Start);

    [JsonIgnore]
    public double Second => Time.TotalSeconds;
}

/// <summary>
/// 字幕字詞片段，記錄了一句字幕中，特定語詞的起始時間。
/// </summary>
public class SubtitleLineWordSegment
{
    /// <summary>
    /// 語詞。
    /// </summary>
    public string Word { get; set; } = string.Empty;

    /// <summary>
    /// 開始時間，格式為 hh:mm:ss.fff
    /// </summary>
    public string Start { get; set; } = string.Empty;

    public SubtitleLineWordSegment() { }

    public SubtitleLineWordSegment(TimeSpan time, string word)
    {
        Start = time.ToString("hh\\:mm\\:ss\\.fff");
        Word = word;
    }
}

public class SubtitleModifiedState
{
    public string Action { get; set; } = string.Empty;
    public object Data { get; set; } = null!;
    public bool UndoExecuted { get; set; }
}