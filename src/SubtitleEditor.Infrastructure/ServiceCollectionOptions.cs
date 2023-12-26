namespace SubtitleEditor.Infrastructure;

public class ServiceCollectionOptions
{
    /// <summary>
    /// 是否要將系統設定存放在 Storage 的設定檔資料夾內。如果提供 <see langword="false"/>，則會使用資料庫來儲存系統設定。
    /// </summary>
    public bool UseSystemOptionConfiguration { get; set; }

    /// <summary>
    /// 是否使用交大的辨識服務 API 所提供的勘誤表。如果提供 <see langword="false"/>，則會使用資料庫內的勘誤表設定。
    /// </summary>
    public bool UseNctuFixBook { get; set; }
}