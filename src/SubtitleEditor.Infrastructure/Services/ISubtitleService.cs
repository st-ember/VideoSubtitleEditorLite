using SubtitleEditor.Core.Models;
using SubtitleEditor.Infrastructure.Models.Asr;

namespace SubtitleEditor.Infrastructure.Services;

public interface ISubtitleService
{
    Task<string> ReadFileToStringAsync(byte[] file);
    Task<Subtitle> ReadFileAsync(byte[] file);
    Task<Subtitle> ReadFileAsync(byte[] file, double frameRate);
    Subtitle ParseFromSrt(string srt);
    Subtitle ParseFromVtt(string vtt);
    Subtitle ParseFromInline(string inline);

    /// <summary>
    /// 將字幕物件轉成純文字的 SRT 格式。
    /// </summary>
    /// <param name="subtitle">字幕物件</param>
    /// <param name="frameRate">影片的每秒畫格數，如提供此值會將不足一秒的時間換算成畫格數，並改用逗號分隔時間及畫格數。</param>
    /// <returns>SRT 格式的文字</returns>
    string ToSrt(Subtitle subtitle, double? frameRate = null);

    /// <summary>
    /// 將字幕物件轉成純文字的 VTT 格式。如字幕物件包含檔頭，也會一併被加入到匯出的文字開頭。
    /// </summary>
    /// <param name="subtitle">字幕物件</param>
    /// <param name="frameRate">影片的每秒畫格數，如提供此值會將不足一秒的時間換算成畫格數，並改用逗號分隔時間及畫格數。</param>
    /// <returns>VTT 格式的文字</returns>
    string ToVtt(Subtitle subtitle, double? frameRate = null);

    /// <summary>
    /// 將字幕物件轉換成純文字內容，每一行包含一句字幕的起始時間與字幕文字。
    /// </summary>
    /// <param name="subtitle">字幕物件</param>
    /// <param name="frameRate">影片的每秒畫格數，如提供此值會將不足一秒的時間換算成畫格數，並改用逗號分隔時間及畫格數。</param>
    /// <returns>純文字內容</returns>
    string ToInline(Subtitle subtitle, double? frameRate = null);

    /// <summary>
    /// 將字幕物件轉換成無時間的純文字內容，每一行包含一句字幕文字。
    /// </summary>
    /// <param name="subtitle">字幕物件</param>
    /// <returns>純文字內容</returns>
    string ToNoTime(Subtitle subtitle);

    /// <summary>
    /// 將字詞時戳套入字幕物件內，此方法會嘗試逐句比對所有字幕以及對應的字詞時戳。碰到連續的英文單字時會在單字間插入空白。
    /// </summary>
    /// <param name="subtitle">字幕物件</param>
    /// <param name="nctuWordSegments">字詞時戳物件陣列</param>
    /// <returns>完成套用字詞時戳的字幕物件</returns>
    Subtitle UpdateSubtitleWithWordSegments(Subtitle subtitle, NctuWordSegment[] nctuWordSegments);

    /// <summary>
    /// 批次取代字幕物件內的文字。
    /// </summary>
    /// <param name="subtitle">字幕物件</param>
    /// <param name="replaceMap">要取代的文字對照字典</param>
    void ReplaceInSubtitle(Subtitle subtitle, Dictionary<string, string> replaceMap);
}
