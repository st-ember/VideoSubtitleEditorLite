using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEditor.Core.Contexts
{
    public enum CreatedOption
    {
        [Description("辨識流程")]
        AIGenerated,
        [Description("上傳字幕")]
        SubtitleUpload,
        [Description("字幕時碼匹配")]
        SrtGenerated,
        [Description("逐字稿上傳")]
        TranscriptUpload
    }
}
