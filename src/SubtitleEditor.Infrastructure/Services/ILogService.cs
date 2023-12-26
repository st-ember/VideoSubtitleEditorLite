using Microsoft.AspNetCore.Mvc.Filters;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Models;

namespace SubtitleEditor.Infrastructure.Services;

public interface ILogService
{
    /// <summary>
    /// 當次操作代表的 Log 資料庫 Entity 物件。
    /// </summary>
    Log? Log { get; }

    /// <summary>
    /// 當次操作名稱/代碼。
    /// </summary>
    string ActionText { get; set; }

    /// <summary>
    /// 當次操作的訊息。
    /// </summary>
    string? Message { get; set; }

    /// <summary>
    /// 當次操作的目標物件顯示名稱。
    /// </summary>
    string? Target { get; set; }

    /// <summary>
    /// 當次操作對於目標物件進行變更的欄位顯示名稱。
    /// </summary>
    string? Field { get; set; }

    /// <summary>
    /// 當次操作之前的資料。
    /// </summary>
    string? Before { get; set; }

    /// <summary>
    /// 當次操作完成後的資料。
    /// </summary>
    string? After { get; set; }

    /// <summary>
    /// 當次操作的結果代碼。
    /// </summary>
    string? Code { get; set; }

    /// <summary>
    /// 執行操作的使用者 ID。
    /// </summary>
    Guid? UserId { get; set; }

    /// <summary>
    /// 這次操作提出的要求資料。
    /// </summary>
    object? Request { set; }

    /// <summary>
    /// 這次操作的結果資料。
    /// </summary>
    object? Response { set; }

    /// <summary>
    /// 當次操作的操作 ID。
    /// </summary>
    Guid? ActionId { get; }

    /// <summary>
    /// 是否只有在發生錯誤時才將當次操作寫入到資料庫內。
    /// </summary>
    bool OnlyLogOnError { get; set; }

    /// <summary>
    /// 建立一筆新的子 Log 物件。
    /// </summary>
    Log GenerateNewLog();

    /// <summary>
    /// 開始一個可被記錄的操作。
    /// </summary>
    /// <param name="logAction">操作的名稱/代碼</param>
    /// <param name="func">要執行的操作，以 Action 的方式輸入。</param>
    /// <returns>執行結果</returns>
    Task<ILogResult> StartAsync(string logAction, Action func);

    /// <summary>
    /// 開始一個可被記錄的非同步操作。
    /// </summary>
    /// <param name="logAction">操作的名稱/代碼</param>
    /// <param name="func">要執行的非同步操作，以 Func 的方式輸入。</param>
    /// <returns>執行結果</returns>
    Task<ILogResult> StartAsync(string logAction, Func<Task> func);

    /// <summary>
    /// 開始一個可被記錄且帶有回傳值的操作。
    /// </summary>
    /// <typeparam name="T">回傳值的型別</typeparam>
    /// <param name="logAction">操作的名稱/代碼</param>
    /// <param name="func">要執行的操作，以 Action 的方式輸入。</param>
    /// <returns>執行結果</returns>
    Task<ILogResult<T>> StartAsync<T>(string logAction, Func<T> func);

    /// <summary>
    /// 開始一個可被記錄且帶有回傳值的非同步操作。
    /// </summary>
    /// <typeparam name="T">回傳值的型別</typeparam>
    /// <param name="logAction">操作的名稱/代碼</param>
    /// <param name="func">要執行的非同步操作，以 Func 的方式輸入。</param>
    /// <returns>執行結果</returns>
    Task<ILogResult<T>> StartAsync<T>(string logAction, Func<Task<T>> func);

    /// <summary>
    /// 開始一個可被記錄且帶有回傳值的非同步操作，此方法專門提供給 Action Filter 紀錄操作使用。
    /// </summary>
    /// <param name="logAction">操作的名稱/代碼</param>
    /// <param name="func"></param>
    Task<ILogResult<ActionExecutedContext>> StartAsync(string logAction, Func<Task<ActionExecutedContext>> func);

    /// <summary>
    /// 開始一個可被記錄的操作。
    /// </summary>
    /// <typeparam name="TEnum">代表操作名稱的列舉物件型別</typeparam>
    /// <param name="logAction">操作的名稱/代碼</param>
    /// <param name="func">要執行的操作，以 Action 的方式輸入。</param>
    /// <returns>執行結果</returns>
    Task<ILogResult> StartAsync<TEnum>(TEnum logAction, Action func) where TEnum : Enum;

    /// <summary>
    /// 開始一個可被記錄的非同步操作。
    /// </summary>
    /// <typeparam name="TEnum">代表操作名稱的列舉物件型別</typeparam>
    /// <param name="logAction">操作的名稱/代碼</param>
    /// <param name="func">要執行的非同步操作，以 Func 的方式輸入。</param>
    /// <returns>執行結果</returns>
    Task<ILogResult> StartAsync<TEnum>(TEnum logAction, Func<Task> func) where TEnum : Enum;

    /// <summary>
    /// 開始一個可被記錄且帶有回傳值的操作。
    /// </summary>
    /// <typeparam name="T">回傳值的型別</typeparam>
    /// <typeparam name="TEnum">代表操作名稱的列舉物件型別</typeparam>
    /// <param name="logAction">操作的名稱/代碼</param>
    /// <param name="func">要執行的操作，以 Action 的方式輸入。</param>
    /// <returns>執行結果</returns>
    Task<ILogResult<T>> StartAsync<T, TEnum>(TEnum logAction, Func<T> func) where TEnum : Enum;

    /// <summary>
    /// 開始一個可被記錄且帶有回傳值的非同步操作。
    /// </summary>
    /// <typeparam name="T">回傳值的型別</typeparam>
    /// <typeparam name="TEnum">代表操作名稱的列舉物件型別</typeparam>
    /// <param name="logAction">操作的名稱/代碼</param>
    /// <param name="func">要執行的非同步操作，以 Func 的方式輸入。</param>
    /// <returns>執行結果</returns>
    Task<ILogResult<T>> StartAsync<T, TEnum>(TEnum logAction, Func<Task<T>> func) where TEnum : Enum;

    /// <summary>
    /// 開始一個可被記錄且帶有回傳值的非同步操作，此方法專門提供給 Action Filter 紀錄操作使用。
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="logAction">操作的名稱/代碼</param>
    /// <param name="func"></param>
    Task<ILogResult<ActionExecutedContext>> StartAsync<TEnum>(TEnum logAction, Func<Task<ActionExecutedContext>> func) where TEnum : Enum;

    /// <summary>
    /// 加入一筆額外的紀錄。
    /// </summary>
    /// <param name="log">紀錄物件。</param>
    Log AppendLog(Log log);

    /// <summary>
    /// 紀錄一筆額外的操作紀錄。
    /// </summary>
    /// <param name="message">訊息</param>
    /// <param name="target">目標顯示名稱</param>
    /// <param name="field">目標欄位顯示名稱</param>
    /// <param name="before">變更前的資料</param>
    /// <param name="after">變更後的資料</param>
    Log SystemInfo(string message, string? target = "", string? field = "", string? before = "", string? after = "");

    /// <summary>
    /// 紀錄一筆額外的錯誤訊息。
    /// </summary>
    /// <param name="message">錯誤訊息</param>
    Log SystemError(string message);

    /// <summary>
    /// 記錄一筆額外的例外。
    /// </summary>
    /// <param name="exception">例外物件</param>
    Log SystemError(Exception exception);

    /// <summary>
    /// 提早在 End 之前將所有紀錄寫入資料庫。會觸發資料庫的 SaveChanges()
    /// </summary>
    /// <returns></returns>
    Task SaveLogAsync();
}