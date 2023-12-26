namespace SubtitleEditor.Core.Attributes;

/// <summary>
/// 列舉項目的別名。
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class AliasAttribute : Attribute
{
    /// <summary>
    /// 已設定的別名
    /// </summary>
    public string[] Aliases => _aliases;
    private readonly string[] _aliases = Array.Empty<string>();

    /// <summary>
    /// 設定單一個字串的別名。
    /// </summary>
    /// <param name="alias"></param>
    public AliasAttribute(string alias)
    {
        _aliases = new string[] { alias };
    }

    /// <summary>
    /// 輸入一個可列舉清單做為別名。
    /// </summary>
    /// <param name="aliases"></param>
    public AliasAttribute(IEnumerable<string> aliases)
    {
        _aliases = aliases.ToArray();
    }

    /// <summary>
    /// 輸入多個字串做為別名。
    /// </summary>
    /// <param name="aliases"></param>
    public AliasAttribute(params string[] aliases)
    {
        _aliases = aliases.ToArray();
    }
}
