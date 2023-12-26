namespace SubtitleEditor.Core.Attributes;

/// <summary>
/// 操作訊息。此屬性描述的訊息可以用來顯示在前端，或是記錄在 Log 中以更明確操作的細節。
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ActionMessageAttribute : Attribute
{
    /// <summary>
    /// 訊息文字
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 提供操作代表的訊息文字來建立此屬性。
    /// </summary>
    /// <param name="message"></param>
    public ActionMessageAttribute(string message)
    {
        Message = message;
    }
}
