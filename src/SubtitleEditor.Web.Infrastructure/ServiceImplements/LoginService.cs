using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Database;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.Infrastructure.Services;
using System.Security.Claims;

namespace SubtitleEditor.Web.Infrastructure.ServiceImplements;

public class LoginService : ILoginService
{
    public static readonly string AuthenticationScheme = "VSEInstance";

    private readonly HttpContext? _httpContext;
    private readonly EditorContext _database;
    private readonly IEncryptService _encryptService;
    private readonly ILogService _logService;
    private readonly IPermissionService _permissionService;
    private readonly IActivationService _activationService;
    private readonly ISessionService _sessionService;

    public LoginService(
        IHttpContextAccessor httpContextAccessor,
        EditorContext database,
        IEncryptService encryptService,
        ILogService logService,
        IPermissionService permissionService,
        IActivationService activationService,
        ISessionService sessionService
        )
    {
        _httpContext = httpContextAccessor.HttpContext;
        _database = database;
        _encryptService = encryptService;
        _logService = logService;
        _permissionService = permissionService;
        _activationService = activationService;
        _sessionService = sessionService;
    }

    public Guid? GetLoginId()
    {
        return _httpContext != null && _httpContext.User.Identity != null && _httpContext.User.Identity.IsAuthenticated &&
            Guid.TryParse(_httpContext.User.Claims.Where(o => o.Type == ClaimTypes.Thumbprint).Select(o => o.Value).Single(), out var id) ? id : null;
    }

    public async Task<User> CheckAndLogInAsync(string account, string password)
    {
        await _ensureDefaultAccount();

        var adoptedAccount = !string.IsNullOrWhiteSpace(account) ? account.ToUpper().Trim() : "";
        var user = await _database.Users
            .AsNoTracking()
            .Where(e => e.Account.ToUpper() == adoptedAccount && e.Status != UserStatus.Removed)
            .FirstOrDefaultAsync();

        if (user != null)
        {
            var permissionContext = await _permissionService.GetUserPermissionAsync(user.Id);
            if (!permissionContext.Contains(SystemAction.Login))
            {
                throw new UnauthorizedAccessException("您沒有執行此作業的權限！");
            }
        }

        var login = await _checkLoginRequestAsync(user, password);
        await _checkActivatedSessionAsync();
        await _checkSecurityAsync(user!);
        await LoginInCookieAsync(user!, login);

        return user!;
    }

    public async Task LoginInCookieAsync(User user, UserLogin? login = null)
    {
        if (_httpContext != null)
        {
            var adoptedLogin = login;
            if (adoptedLogin == null)
            {
                adoptedLogin = new UserLogin
                {
                    Success = true,
                    IPAddress = _httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "",
                    UserId = user.Id,
                    ActionId = _logService.ActionId ?? default
                };

                await _saveLoginAsync(adoptedLogin);
            }

            await _httpContext!.SignInAsync(
                AuthenticationScheme,
                new ClaimsPrincipal(_buildClaimsIdentity(user, adoptedLogin)),
                new AuthenticationProperties() { });
        }
    }

    public async Task LogoutAsync()
    {
        if (_httpContext != null && _httpContext.User.Identity != null && _httpContext.User.Identity.IsAuthenticated)
        {
            try
            {
                var loginId = GetLoginId() ?? default;

                var login = await _database.UserLogins
                    .Where(e => e.Id == loginId && !e.DueTime.HasValue)
                    .SingleOrDefaultAsync();

                if (login != null)
                {
                    login.DueTime = DateTime.Now;
                    await _database.SaveChangesAsync();
                }
            }
            finally
            {
                await _httpContext.SignOutAsync(AuthenticationScheme);
            }
        }
    }

    private async Task<UserLogin> _checkLoginRequestAsync(User? user, string password)
    {
        var failureLoginTimePeriod = DateTime.Now.AddMinutes(-15);

        var ipAddress = _httpContext?.Connection.RemoteIpAddress?.ToString() ?? "";
        var recentLoginFromSameIPAddress = await _database.Logs
            .AsNoTracking()
            .Where(e =>
                e.ActionText == SystemAction.Login.ToString() &&
                e.Time > failureLoginTimePeriod &&
                e.IPAddress == ipAddress)
            .ToArrayAsync();

        var ipAddressFailedCount = 0;
        for (var i = 0; i < recentLoginFromSameIPAddress.Length; i++)
        {
            var recentLogin = recentLoginFromSameIPAddress[i];
            if (recentLogin.Success)
            {
                ipAddressFailedCount = 0;
            }
            else
            {
                ipAddressFailedCount++;
                if (ipAddressFailedCount >= 3)
                {
                    throw new Exception("於 15 分鐘內登入失敗超過三次！");
                }
            }
        }

        if (user == null)
        {
            throw new Exception("無效的帳號或密碼！");
        }

        var recentLogins = await _database.UserLogins
            .Where(e => e.UserId == user.Id && e.Time > failureLoginTimePeriod)
            .ToArrayAsync();

        var failedCount = 0;
        for (var i = 0; i < recentLogins.Length; i++)
        {
            var recentLogin = recentLogins[i];
            if (recentLogin.Success)
            {
                failedCount = 0;
            }
            else
            {
                failedCount++;
                if (failedCount >= 3)
                {
                    await _saveFailureLoginRecordAsync(user.Id, "帳號於 15 分鐘內連續登入失敗超過三次！");
                    throw new Exception("帳號於 15 分鐘內連續登入失敗超過三次！");
                }
            }
        }

        var validPassword = _encryptService.Decrypt(user.Password) == password;
        if (!validPassword)
        {
            await _saveFailureLoginRecordAsync(user.Id, "密碼錯誤！");
            throw new Exception("無效的帳號或密碼！");
        }

        if (user.Status != UserStatus.Enabled)
        {
            var message = user.Status == UserStatus.Disabled ? "帳號尚未啟用！" : "帳號不存在！";
            await _saveFailureLoginRecordAsync(user.Id, message);
            throw new Exception("帳號尚未啟用或狀態錯誤！");
        }

        var login = new UserLogin
        {
            Success = true,
            IPAddress = _httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "",
            UserId = user.Id,
            ActionId = _logService.ActionId ?? default
        };

        await _saveLoginAsync(login);

        return login;
    }

    private static ClaimsIdentity _buildClaimsIdentity(User user, UserLogin login)
    {
        var identity = new ClaimsIdentity(AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.Sid, user.Id.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Name, (user.Account ?? "").Trim()));
        identity.AddClaim(new Claim(ClaimTypes.GivenName, (user.Name ?? "").Trim()));
        identity.AddClaim(new Claim(ClaimTypes.Thumbprint, login.Id.ToString()));
        return identity;
    }

    private Task _saveFailureLoginRecordAsync(Guid userId, string message)
    {
        return _saveLoginAsync(new UserLogin
        {
            Success = false,
            Message = message,
            IPAddress = _httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "",
            UserId = userId,
            ActionId = _logService.ActionId ?? default
        });
    }

    private async Task _saveLoginAsync(UserLogin login)
    {
        _database.UserLogins.Add(login);
        await _database.SaveChangesAsync();
        _database.Detach(login);
    }

    private async Task _checkSecurityAsync(User user)
    {
        var userSecCount = await _database.UserSecrets
            .AsNoTracking()
            .Where(e => e.UserId == user.Id)
            .CountAsync();

        if (userSecCount == 0)
        {
            // 如果沒有找到任何安全資訊，表示帳號從沒有在新的安全機制上線後登入過。需要建立第一筆安全資訊。
            _database.UserSecrets.Add(new UserSecret()
            {
                UserId = user.Id,
                Value = user.Password,
                Reason = UserSecretCreateReason.SystemApply
            });
            await _database.SaveChangesAsync();
        }
    }

    private async Task _checkActivatedSessionAsync()
    {
        var calCount = await _activationService.GetCalCountAsync();
        if (!calCount.HasValue)
        {
            return;
        }

        var currentSessionCount = _sessionService.GetCurrentSessionCount();
        if (currentSessionCount >= calCount.Value && calCount.Value != 0)
        // calCount.Value == 0 的話，代表ActivationKey設定為無上限，一樣通過。
        {
            throw new Exception("當前的使用者容量已滿，此容量取決於授權的 cal 數量。");
        }
    }

    /// <summary>
    /// 檢查資料庫內是否有任何帳號。
    /// 當資料庫一開始被建立時是沒有任何帳號的，這時候會需要建立一個預設帳號。
    /// </summary>
    private async Task _ensureDefaultAccount()
    {
        if (await _database.Users.AnyAsync())
        {
            return;
        }

        var madminPassword = _encryptService.Encrypt("123");
        var defaultAccount = new User
        {
            Account = "MADMIN",
            Password = madminPassword,
            Name = "預設管理員帳號",
            Secrets = new HashSet<UserSecret>
            {
                new UserSecret
                {
                    Value = madminPassword,
                    Reason = UserSecretCreateReason.SystemApply,
                    ValidTo = DateTime.Today.AddDays(60)
                }
            }
        };

        var defaultUserGroup = new UserGroup
        {
            Name = "系統管理員",
            Description = "系統預設的系統管理員群組，此群組的使用者可以存取所有權限，不受權限設定限制。",
            GroupType = PermissionType.SystemAdmin
        };

        var defaultUserGroupLink = new UserGroupLink
        {
            UserId = defaultAccount.Id,
            UserGroupId = defaultUserGroup.Id,
        };

        _database.Users.Add(defaultAccount);
        _database.UserGroups.Add(defaultUserGroup);
        _database.UserGroupLinks.Add(defaultUserGroupLink);

        await _database.SaveChangesAsync();

        _database.Detach(defaultAccount);
        _database.Detach(defaultUserGroup);
        _database.Detach(defaultUserGroupLink);
    }
}