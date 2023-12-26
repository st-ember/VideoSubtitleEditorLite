using Microsoft.AspNetCore.SignalR;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.Infrastructure.Services;

namespace SubtitleEditor.Web.Hubs;

public class SessionHub : Hub
{
    private readonly ISessionService _sessionService;
    private readonly IAccountService _accountService;
    private readonly ILogService _logService;
    private readonly IActivationService _activationService;

    public SessionHub(
        ISessionService sessionService,
        IAccountService accountService,
        ILogService logService,
        IActivationService activationService
        )
    {
        _sessionService = sessionService;
        _accountService = accountService;
        _logService = logService;
        _activationService = activationService;
    }

    public void ReloginAllConnection()
    {
        if (_accountService.TryGetLoginUserId(out var userId))
        {
            var connectionIds = _sessionService.ListConnectionId(userId);
            Clients.Clients(connectionIds).SendAsync("Relogin");
        }
    }

    public override async Task OnConnectedAsync()
    {
        if (_accountService.TryGetLoginUserId(out var userId))
        {
            try
            {
                _sessionService.Connect(userId, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logService.SystemError(ex);
            }

            await base.OnConnectedAsync();

            var calCount = await _activationService.GetCalCountAsync();
            if (calCount.HasValue)
            {
                var currentSessionCount = _sessionService.GetCurrentSessionCount();
                if (currentSessionCount > calCount.Value && calCount.Value != 0)
                // calCount == 0 代表ActivationKey設定為無上限，一樣通過。
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("Logout");
                }
            }
        }
        else
        {
            await base.OnConnectedAsync();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_accountService.TryGetLoginUserId(out var userId))
        {
            try
            {
                _sessionService.Disconnect(userId, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logService.SystemError(ex);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
