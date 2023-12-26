using SubtitleEditor.Core.Attributes;

namespace SubtitleEditor.Core.Contexts;

public enum UserKeybindingActions
{
    [Name("完成字幕")]
    FinishLine,

    [Name("開始修改字幕")]
    EditLine,

    [Name("刪除所選字幕")]
    DeleteLine,

    [Name("播放媒體")]
    Play,

    [Name("暫停媒體")]
    Pause,

    [Name("播放字幕區間")]
    PlayPeriod,

    [Name("分割字幕")]
    Split,

    [Name("合併所選字幕")]
    Marge,

    [Name("與前一句合併")]
    MargePrev,

    [Name("與後一句合併")]
    MargeNext,

    [Name("編輯前一句")]
    EditPrev,

    [Name("編輯後一句")]
    EditNext,

    [Name("選擇前一句")]
    SelectPrev,

    [Name("選擇後一句")]
    SelectNext,

    [Name("增加選擇前一句")]
    AddSelectPrev,

    [Name("增加選擇後一句")]
    AddSelectNext,

    [Name("在本句前插入一句")]
    InsertBefore,

    [Name("在本句後插入一句")]
    InsertAfter,

    [Name("全選")]
    SelectAll,

    [Name("搜尋")]
    Search,

    [Name("推移時間")]
    ShiftTime,

    [Name("還原回原始資料")]
    RecoverToOriginal,

    [Name("上一步")]
    Undo,

    [Name("下一步")]
    Redo,

    [Name("快速存檔")]
    Save,

    [Name("從逐字稿快速建立新行")]
    QuickCreateLine,

    [Name("播放器回推一秒")]
    PrevSecond,

    [Name("播放器前進一秒")]
    NextSecond
}