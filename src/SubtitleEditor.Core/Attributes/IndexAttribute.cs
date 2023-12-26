namespace SubtitleEditor.Core.Attributes;

/// <summary>
/// 指定 Field 的 Index 值。
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class IndexAttribute : Attribute
{
    /// <summary>
    /// Index
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 指定 Field 的 Index 值。
    /// </summary>
    /// <param name="index">Index</param>
    public IndexAttribute(int index)
    {
        Index = index;
    }
}