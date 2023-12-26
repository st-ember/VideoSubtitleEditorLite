namespace SubtitleEditor.Core.Abstract;

/// <summary>
/// 此物件包含 ID。
/// </summary>
public interface IWithId
{
    /// <summary>
    /// 取得此物件的 ID
    /// </summary>
    object GetId();

    /// <summary>
    /// 比較輸入的 ID 是否與此物件的 ID 相同。
    /// </summary>
    bool HasSameId(object id);
}

/// <summary>
/// 此物件包含泛型型別指定的 ID。
/// </summary>
/// <typeparam name="TId">ID 的型別</typeparam>
public interface IWithId<TId> : IWithId
{
    TId Id { get; set; }

    /// <summary>
    /// 取得此物件的 ID
    /// </summary>
    TId GetGenericId();

    /// <summary>
    /// 比較輸入的 ID 是否與此物件的 ID 相同。
    /// </summary>
    bool HasSameId(TId id);
}
