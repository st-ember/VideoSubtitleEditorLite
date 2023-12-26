using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Database;
using SubtitleEditor.Infrastructure.Services;
using System.Security.Claims;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class AccountService : IAccountService
{
    private readonly HttpContext _httpContext;
    private readonly EditorContext _database;

    public AccountService(
        IHttpContextAccessor httpContextAccessor,
        EditorContext database
        )
    {
        _httpContext = httpContextAccessor.HttpContext!;
        _database = database;
    }

    public bool IsLogined()
    {
        return _httpContext != default && _httpContext.User.Identity != null && _httpContext.User.Identity.IsAuthenticated;
    }

    public Guid? GetLoginUserId()
    {
        return TryGetLoginUserId(out var userId) ? userId : null;
    }

    public Guid? GetLoginId()
    {
        return TryGetLoginId(out var userId) ? userId : null;
    }

    public bool TryGetLoginUserId(out Guid userId)
    {
        if (!IsLogined())
        {
            userId = default;
            return false;
        }

        var idString = _httpContext.User.Claims.Where(o => o.Type == ClaimTypes.Sid).Select(o => o.Value).FirstOrDefault();
        userId = !string.IsNullOrWhiteSpace(idString) && Guid.TryParse(idString, out Guid id) ? id : default;

        return userId != default;
    }

    public bool TryGetLoginId(out Guid userId)
    {
        if (!IsLogined())
        {
            userId = default;
            return false;
        }

        var idString = _httpContext.User.Claims.Where(o => o.Type == ClaimTypes.Thumbprint).Select(o => o.Value).FirstOrDefault();
        userId = !string.IsNullOrWhiteSpace(idString) && Guid.TryParse(idString, out Guid id) ? id : default;

        return userId != default;
    }

    public string? GetUserIPAddress()
    {
        return _httpContext != null ? _httpContext.Connection.RemoteIpAddress?.ToString() : default;
    }

    public string? GetClaimValue(string claimName)
    {
        return IsLogined() ?
            _httpContext.User.Claims.Where(o => o.Type == claimName).Select(o => o.Value).FirstOrDefault() :
            default;
    }

    public IEnumerable<string> GetClaimValues(string claimName)
    {
        return IsLogined() ?
            _httpContext.User.Claims.Where(o => o.Type == claimName).Select(o => o.Value) :
            Array.Empty<string>();
    }

    public string? GetLoginUserAccount()
    {
        return GetClaimValue(ClaimTypes.Name);
    }

    public async Task<bool> IsUserPasswordExpiredAsync(Guid userId, int expireDays)
    {
        var securityRecord = await _database.UserSecrets
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Create)
            .FirstOrDefaultAsync();

        return securityRecord == null || (DateTime.Today - securityRecord.Create.Date).TotalDays > expireDays;
    }
}