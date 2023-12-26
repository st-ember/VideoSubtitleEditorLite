namespace SubtitleEditor.Infrastructure.Services;

public interface IAccountService
{
    /// <summary>
    /// 取得已登入使用者的 ID。
    /// </summary>
    /// <returns>GUID 格式的 ID，如果使用者未登入則回傳 null。</returns>
    Guid? GetLoginUserId();

    /// <summary>
    /// 取得登入階段的 ID。
    /// </summary>
    /// <returns>GUID 格式的 ID，如果使用者未登入則回傳 null。</returns>
    Guid? GetLoginId();

    bool TryGetLoginUserId(out Guid userId);
    bool TryGetLoginId(out Guid userId);

    /// <summary>
    /// 使用者是否已經登入。
    /// </summary>
    bool IsLogined();

    /// <summary>
    /// 取得目前使用者的 IP。
    /// </summary>
    string? GetUserIPAddress();

    /// <summary>
    /// 取得第一個吻合 Claim 名稱的值。
    /// </summary>
    /// <param name="claimName"></param>
    string? GetClaimValue(string claimName);

    /// <summary>
    /// 取得吻合 Cliam 名稱的所有值。
    /// </summary>
    /// <param name="claimName"></param>
    IEnumerable<string> GetClaimValues(string claimName);

    /// <summary>
    /// 取得已登入使用者的帳號。
    /// </summary>
    string? GetLoginUserAccount();

    /// <summary>
    /// 檢查指定使用者的密碼是否已經過期。
    /// </summary>
    /// <param name="userId">使用者的 ID</param>
    /// <returns>密碼是否過期</returns>
    Task<bool> IsUserPasswordExpiredAsync(Guid userId, int expireDays);
}