using SubtitleEditor.Database.Entities;

namespace SubtitleEditor.Web.Infrastructure.Services;

public interface ILoginService
{
    /// <summary>
    /// 如果使用者已經登入，取得使用者的 LoginId。
    /// </summary>
    Guid? GetLoginId();

    /// <summary>
    /// 檢查輸入的帳號密碼是否符合且使用者狀態是否可以正常登入。如果可以登入，則進行登入；如果發生問題則回拋例外。
    /// </summary>
    /// <param name="account">帳號</param>
    /// <param name="password">密碼</param>
    /// <returns>成功登入後回傳使用者實體</returns>
    Task<User> CheckAndLogInAsync(string account, string password);

    /// <summary>
    /// 將指定的使用者登入。
    /// </summary>
    /// <param name="user">包含角色、系統角色和使用者安全性關聯物件的使用者實體。</param>
    Task LoginInCookieAsync(User user, UserLogin? login = null);

    /// <summary>
    /// 將目前的使用者登出。
    /// </summary>
    Task LogoutAsync();
}