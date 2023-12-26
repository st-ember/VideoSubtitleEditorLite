using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.Hubs;
using SubtitleEditor.Web.Infrastructure.Services;
using SubtitleEditor.Web.Models.Account;

namespace SubtitleEditor.Web.Controllers;

public class AccountController : AuthorizedController
{
    private readonly HttpContext? _httpContext;
    private readonly ILogService _logService;
    private readonly IAccountService _accountService;
    private readonly ILoginService _loginService;
    private readonly IUserService _userService;
    private readonly ISystemOptionService _systemOptionService;
    private readonly ISessionService _sessionService;
    private readonly IHubContext<SessionHub> _hubContext;
    private readonly IActivationService _activationService;
    public AccountController(
        IActivationService activationService,
        IHttpContextAccessor httpContextAccessor,
        ILogService logService,
        IAccountService accountService,
        ILoginService loginService,
        IUserService userService,
        ISystemOptionService systemOptionService,
        ISessionService sessionService,
        IHubContext<SessionHub> hubContext
        )
    {
        _httpContext = httpContextAccessor.HttpContext;
        _logService = logService;
        _accountService = accountService;
        _loginService = loginService;
        _userService = userService;
        _systemOptionService = systemOptionService;
        _sessionService = sessionService;
        _hubContext = hubContext;
        _activationService = activationService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Login(string returnUrl = @"/")
    {
        var key = await _systemOptionService.GetContentAsync(SystemOptionNames.ActivationKey);
        var activationData = _activationService.ResolveKey(key);
        var asrAccess = _activationService.CheckAsrAccess(activationData);
        await _loginService.LogoutAsync();
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = "/")
    {
        //await _activationService.ClearActivationDataAsync();
        if (model == null)
        {
            return View(new LoginViewModel());
        }

        if (_httpContext == null)
        {
            return View(new LoginViewModel { Account = model.Account, Error = "連線錯誤！" });
        }

        if (string.IsNullOrEmpty(model.Account))
        {
            return View(new LoginViewModel { Error = "帳號欄位不得為空！" });
        }

        if (string.IsNullOrEmpty(model.Password))
        {
            return View(new LoginViewModel
            {
                Account = model.Account,
                Error = "密碼欄位不得為空！"
            });
        }

        var code = _httpContext.Session.GetString("CaptchaCode");
        _httpContext.Session.Remove("CaptchaCode");

        if (string.IsNullOrWhiteSpace(model.CaptchaCode) || model.CaptchaCode.ToUpper() != code)
        {
            return View(new LoginViewModel
            {
                Account = model.Account,
                Error = "驗證碼錯誤！"
            });
        }

        model.CaptchaCode = "";

        var needResetPassword = false;
        var result = await _logService.StartAsync(
            SystemAction.Login,
            async () =>
            {
                _logService.Target = model.Account;

                var user = await _loginService.CheckAndLogInAsync(model.Account, model.Password);

                _logService.UserId = user.Id;
                _logService.Target = user.Account;

                var expireDays = await _systemOptionService.GetIntAsync(SystemOptionNames.PasswordExpireDays) ?? 0;
                if (expireDays > 0)
                {
                    needResetPassword = await _accountService.IsUserPasswordExpiredAsync(user.Id, expireDays);
                }
            });

        if (result.Success)
        {
            if (!needResetPassword)
            {
                return Redirect(returnUrl ?? "/");
            }
            else
            {
                return RedirectToAction("RenewPassword", new { returnUrl });
            }
        }
        else
        {
            model.Error = result.Message;
            return View(model);
        }

    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        if (_accountService.TryGetLoginUserId(out var userId))
        {
            // 取得所有使用者正在使用的連線 ID。
            var connectionIds = _sessionService.ListConnectionId(userId);

            // 正式登出使用者。
            await _loginService.LogoutAsync();

            // 發送訊息給使用者所有的頁面進行同步登出。
            // 這邊很重要的一點，是要在正式登出"後"才對所有頁面下達登出指令，不然所有頁面都只會回到入口網首頁而已。
            await _hubContext.Clients.Clients(connectionIds).SendAsync("Relogin");
        }
        else
        {
            await _loginService.LogoutAsync();
        }
        
        return RedirectToAction("Login");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> RenewPassword(string returnUrl)
    {
        if (_accountService.TryGetLoginUserId(out var userId) && _httpContext != null)
        {
            // 將使用者登出，這樣一來使用者直接輸入網址想跳過重設密碼時就會彈回登入畫面。
            await _loginService.LogoutAsync();

            // 將包含使用者 ID 以及當下時間的資訊寫進 Session，方便使用者提交時進行驗證。
            _httpContext.Session.SetString($"RenewPassword-{userId}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            return View(new RenewPasswordViewModel() { Id = userId, ReturnUrl = returnUrl });
        }
        else
        {
            return RedirectToAction("Login", new { returnUrl });
        }
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> RenewPassword(RenewPasswordViewModel model)
    {
        if (model == default || model.Id == default || _httpContext == null)
        {
            return Redirect(model?.ReturnUrl ?? "/");
        }

        // 使用前端傳來的 ID 嘗試從 Session 取出已經儲存的時間資訊。
        var timeCode = _httpContext.Session.GetString($"RenewPassword-{model.Id}");

        // 嘗試解析被要求重設密碼的時間。
        DateTime? time = DateTime.TryParse(timeCode, out DateTime t) ? t : null;

        // 如果從 Session 中無法取出有效資訊，或是使用者等待超過十分鐘才提交，則直接要求使用者重登。
        if (!time.HasValue || DateTime.Now - time.Value > TimeSpan.FromMinutes(10))
        {
            return Redirect(model.ReturnUrl ?? "/");
        }

        _httpContext.Session.Remove($"RenewPassword-{model.Id}");

        var result = await _logService.StartAsync(
            SystemAction.RenewPassword,
            async () =>
            {
                var user = await _userService.UpdatePasswordAsync(model.Id, model.Password, model.NewPassword, model.Confirm);

                _logService.UserId = user.Id;
                _logService.Target = user.Account;

                await _loginService.LoginInCookieAsync(user);
            });

        if (result.Success)
        {
            return Redirect(model.ReturnUrl ?? "/");
        }
        else
        {
            // 發生錯誤，這邊需要重新將驗證資訊放回 Session 中。
            _httpContext.Session.SetString($"RenewPassword-{model.Id}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

            model.Error = result.Message;
            return View(model);
        }
    }
}
