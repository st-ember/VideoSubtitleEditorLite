using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Abstract;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Database;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Models.User;
using SubtitleEditor.Infrastructure.Services;
using System.Text.RegularExpressions;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class UserService : IUserService
{
    private readonly EditorContext _database;
    private readonly IEncryptService _encryptService;
    private readonly ILogService _logService;
    private readonly ISystemOptionService _systemOptionService;
    private readonly IPermissionService _permissionService;

    public UserService(
        EditorContext database,
        IEncryptService encryptService,
        ILogService logService,
        ISystemOptionService systemOptionService,
        IPermissionService permissionService
        )
    {
        _database = database;
        _encryptService = encryptService;
        _logService = logService;
        _systemOptionService = systemOptionService;
        _permissionService = permissionService;
    }

    public async Task<bool> IsUserPasswordExpiredAsync(Guid userId)
    {
        var securityRecord = await _database.UserSecrets
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Create)
            .FirstOrDefaultAsync();

        if (securityRecord != null)
        {
            return securityRecord.Create.AddDays(60) < DateTime.Now;
        }

        return true;
    }

    public virtual async Task<bool> IsExistAsync(string account)
    {
        if (account == default)
        {
            throw new Exception("無效的 Account");
        }

        var adoptedAccount = account.Trim().ToUpper();
        _logService.Target = adoptedAccount;
        return await _database.Users
            .AnyAsync(e => e.Account.ToUpper() == adoptedAccount && e.Status != UserStatus.Removed);
    }

    public virtual async Task<bool> IsExistAsync(Guid id)
    {
        if (id == default)
        {
            throw new Exception("無效的 ID");
        }

        _logService.Target = id.ToString();
        return await _database.Users
            .AnyAsync(e => e.Id == id && e.Status != UserStatus.Removed);
    }

    public virtual ISimpleResult CheckUser(IUser user)
    {
        if (string.IsNullOrWhiteSpace(user.Account))
        {
            return SimpleResult.IsFailed("帳號不可空白。");
        }

        return SimpleResult.IsSuccess();
    }

    public virtual async Task<User> GetAsync(Guid id)
    {
        if (id == default)
        {
            throw new Exception("無效的 ID");
        }

        _logService.Target = id.ToString();

        var entity = await _database.Users
            .AsNoTracking()
            .Where(e => e.Id == id && e.Status != UserStatus.Removed)
            .Include(e => e.UserGroupLinks)
            .SingleOrDefaultAsync() ?? throw new Exception("找不到使用者");

        _logService.Target = entity.Account;

        return entity;
    }

    public virtual async Task<User[]> ListAsync(IEnumerable<Guid> ids)
    {
        if (ids == null || !ids.Any())
        {
            return Array.Empty<User>();
        }

        var entities = await _database.Users
            .AsNoTracking()
            .Where(e => ids.Contains(e.Id) && e.Status != UserStatus.Removed)
            .ToArrayAsync();

        return entities;
    }

    public virtual async Task<UserLoginRecordData[]> ListLoginRecordAsync(Guid id, DateTime start, DateTime end)
    {
        _logService.Target = id.ToString();

        var adoptedStart = start != default ? start : DateTime.Today.AddDays(-7);
        var adoptedEnd = end != default ? end : DateTime.Today.AddDays(1);
        if (adoptedStart > adoptedEnd)
        {
            adoptedStart = adoptedEnd;
        }

        var logins = await _database.UserLogins
            .AsNoTracking()
            .Where(e => e.UserId == id && start <= e.Time && e.Time <= end)
            .OrderByDescending(e => e.Time)
            .ToArrayAsync();

        return logins
            .Select(e => new UserLoginRecordData
            {
                Id = e.Id,
                IPAddress = e.IPAddress,
                Success = e.Success,
                Message = e.Message,
                Time = e.Time.ToString("yyyy/MM/dd HH:mm:ss"),
                LogoutTime = e.DueTime?.ToString("yyyy/MM/dd HH:mm:ss") ?? ""
            })
            .ToArray();
    }

    public virtual async Task<User> CreateAsync(IUser data, string password, UserStatus status = UserStatus.Enabled)
    {
        var adoptedAccount = data.Account.Trim();
        _logService.Target = adoptedAccount;

        var entity = new User
        {
            Account = adoptedAccount,
            Password = _encryptService.Encrypt(password),
            Name = data.Name,
            Title = data.Title,
            Telephone = data.Telephone,
            Email = data.Email,
            Description = data.Description,
            Status = status,
            Secrets = new List<UserSecret>()
            {
                new UserSecret
                {
                    Value = _encryptService.Encrypt(password),
                    ValidTo = DateTime.Now,
                    Reason = UserSecretCreateReason.SystemApply
                }
            }
        };

        _database.Users.Add(entity);
        await _database.SaveChangesAsync();
        _database.Detach(entity);

        return entity;
    }

    public virtual async Task UpdateAsync(IUser data)
    {
        var modified = false;

        _logService.Target = data.Id.ToString();

        var entity = await GetEntityAsync(data.Id);

        _logService.Target = entity.Account;

        var adoptedAccount = data.Account.Trim();
        var accountModified = !Equals(entity.Account, adoptedAccount, ignoreCase: true);
        if (accountModified && await _database.Users.AnyAsync(e => e.Id != entity.Id && e.Status != UserStatus.Removed && e.Account == adoptedAccount))
        {
            throw new Exception("帳號重複");
        }

        var adoptedName = data.Name?.Trim();
        if (!Equals(entity.Name, adoptedName))
        {
            AddInfo(entity.Account, nameof(entity.Name), entity.Name ?? "-", adoptedName ?? "-");
            entity.Name = adoptedName;
            modified = true;
        }

        var adoptedTitle = data.Title?.Trim();
        if (!Equals(entity.Title, adoptedTitle))
        {
            AddInfo(entity.Account, nameof(entity.Title), entity.Title ?? "-", adoptedTitle ?? "-");
            entity.Title = adoptedTitle;
            modified = true;
        }

        var adoptedPhone = data.Telephone?.Trim();
        if (!Equals(entity.Telephone, adoptedPhone))
        {
            AddInfo(entity.Account, nameof(entity.Telephone), entity.Telephone ?? "-", adoptedPhone ?? "-");
            entity.Telephone = adoptedPhone;
            modified = true;
        }

        var adoptedEmail = data.Email?.Trim();
        if (!Equals(entity.Email, adoptedEmail))
        {
            AddInfo(entity.Account, nameof(entity.Email), entity.Email ?? "-", adoptedEmail ?? "-");
            entity.Email = adoptedEmail;
            modified = true;
        }

        var adoptedDesc = data.Description?.Trim();
        if (!Equals(entity.Description, adoptedDesc))
        {
            AddInfo(entity.Account, nameof(entity.Description), entity.Description ?? "-", adoptedDesc ?? "-");
            entity.Description = adoptedDesc;
            modified = true;
        }

        if (accountModified && !string.IsNullOrEmpty(adoptedAccount))
        {
            AddInfo(entity.Account, nameof(entity.Account), entity.Account, adoptedAccount);
            entity.Account = adoptedAccount;
            modified = true;
        }

        if (modified)
        {
            entity.Update = DateTime.Now;
            await _database.SaveChangesAsync();
        }

        _database.Detach(entity);
    }

    public virtual async Task UpdateUserStatusAsync(Guid id, UserStatus userStatus)
    {
        _logService.Target = id.ToString();

        var entity = await GetEntityAsync(id);
        var modified = false;

        _logService.Target = entity.Account;

        if (userStatus == UserStatus.Removed)
        {
            throw new Exception("無效的狀態");
        }

        if (entity.Status != userStatus)
        {
            AddInfo(entity.Account, nameof(entity.Status), entity.Status.GetName(), userStatus.GetName());
            entity.Status = userStatus;
            modified = true;
        }

        if (modified)
        {
            entity.Update = DateTime.Now;
            await _database.SaveChangesAsync();
            _permissionService.ClearUserPermissionCache(id);
        }

        _database.Detach(entity);
    }

    public virtual async Task UpdateUserGroupAsync(Guid id, IEnumerable<Guid> userGroupIds)
    {
        _logService.Target = id.ToString();

        var entity = await GetEntityAsync(id);
        var modified = false;

        _logService.Target = entity.Account;

        var userGroups = await _database.UserGroups
            .AsNoTracking()
            .ToArrayAsync();

        var selectedUserGroups = userGroups.Where(e => userGroupIds.Contains(e.Id)).ToArray();

        foreach (var link in entity.UserGroupLinks.Where(l => !selectedUserGroups.Any(e => e.Id == l.UserGroupId)))
        {
            var userGroup = userGroups.Where(e => e.Id == link.UserGroupId).Single();
            var log = _logService.GenerateNewLog().SetAction(SystemAction.RemoveUserFromUserGroup);
            log.Target = entity.Account;
            log.Field = userGroup.Name;
            modified = true;
        }

        foreach (var userGroup in selectedUserGroups.Where(e => !entity.UserGroupLinks.Any(l => l.UserGroupId == e.Id)))
        {
            var log = _logService.GenerateNewLog().SetAction(SystemAction.AddUserToUserGroup);
            log.Target = entity.Account;
            log.Field = userGroup.Name;
            modified = true;
        }

        if (modified)
        {
            entity.Update = DateTime.Now;
            entity.UserGroupLinks = selectedUserGroups
                .Select(e => new UserGroupLink
                {
                    UserId = entity.Id,
                    UserGroupId = e.Id
                })
                .ToList();

            await _database.SaveChangesAsync();
            _permissionService.ClearUserPermissionCache(id);
        }

        _database.Detach(entity);
    }

    public virtual async Task<User> UpdatePasswordAsync(Guid id, string newPassword, string confirm)
    {
        var entity = await GetEntityAsync(id);

        _logService.Target = entity.Account;

        var modified = await CheckAndUpdatePasswordAsync(entity, newPassword, confirm);

        if (modified)
        {
            entity.Update = DateTime.Now;
            await _database.SaveChangesAsync();
        }

        _database.Detach(entity);

        return entity;
    }

    public virtual async Task<User> UpdatePasswordAsync(Guid id, string password, string newPassword, string confirm)
    {
        var entity = await GetEntityAsync(id);

        _logService.Target = entity.Account;

        var modified = await CheckAndUpdatePasswordAsync(entity, password, newPassword, confirm);
        if (modified)
        {
            entity.Update = DateTime.Now;
            await _database.SaveChangesAsync();
        }

        _database.Detach(entity);

        return entity;
    }

    public virtual async Task RemoveAsync(Guid id)
    {
        _logService.Target = id.ToString();

        var entity = await GetEntityAsync(id);
        var modified = false;

        _logService.Target = entity.Account;

        if (entity.Status != UserStatus.Removed)
        {
            AddInfo(entity.Account, nameof(entity.Status), entity.Status.GetName(), UserStatus.Removed.GetName());
            entity.Status = UserStatus.Removed;
            modified = true;
        }

        if (modified)
        {
            entity.Update = DateTime.Now;
            await _database.SaveChangesAsync();
            _permissionService.ClearUserPermissionCache(id);
        }

        _database.Detach(entity);
    }

    public virtual async Task<User> ResetPasswordAsync(Guid id, string newPassword)
    {
        _logService.Target = id.ToString();

        var user = await _database.Users
            .Where(e => e.Id == id && e.Status == UserStatus.Enabled)
            .SingleOrDefaultAsync() ?? throw new Exception("找不到使用者！");

        _logService.Target = user.Account;

        user.Password = _encryptService.Encrypt(newPassword);
        user.Update = DateTime.Now;

        AddInfo(user.Account, nameof(user.Password));

        _database.UserSecrets.Add(new UserSecret
        {
            UserId = user.Id,
            Value = _encryptService.Encrypt(newPassword),
            Reason = UserSecretCreateReason.SystemApply
        });

        await _database.SaveChangesAsync();
        _database.Detach(user);

        return user;
    }

    public virtual bool IsValidPassword(string password)
    {
        return Regex.IsMatch(password ?? "", @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[~!@#$%^&*_+`',.:;?={}()><\\.\-\/\|\[\]])[A-Za-z\d~!@#$%^&*_+`',.:;?={}()><\\.\-\/\|\[\]]{8,}$");
    }

    public virtual bool IsValidEmail(string password)
    {
        return Regex.IsMatch(password ?? "", @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,63})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$");
    }

    public virtual string GenerateNewPassword(int length)
    {
        const string alphas = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numbers = "1234567890";
        const string sp = "@!#$%&*^?";

        var selecteds = new List<char>();
        var random = new Random();

        for (var i = 0; i < 2; i++)
        {
            selecteds.Add(sp[random.Next(sp.Length)]);
        }

        var numberLength = (int)Math.Round(((double)length - 2) / 3);
        for (var i = 0; i < numberLength; i++)
        {
            selecteds.Add(numbers[random.Next(numbers.Length)]);
        }

        for (var i = 0; i < length - numberLength - 2; i++)
        {
            selecteds.Add(alphas[random.Next(alphas.Length)]);
        }

        return new string(selecteds.OrderBy(o => random.Next()).ToArray());
    }

    public virtual async Task<bool> CheckPasswordAsync(Guid id, string newPassword, string confirm)
    {
        if (string.IsNullOrEmpty(newPassword))
        {
            throw new Exception("新密碼不可空白！");
        }

        if (newPassword != confirm)
        {
            throw new Exception("新密碼與確認密碼不相等！");
        }

        if (!IsValidPassword(newPassword))
        {
            throw new Exception("密碼安全性不足！至少需要八個字元，且包含大小寫英文、數字與特殊符號。");
        }

        var noneRepeatCount = await _systemOptionService.GetIntAsync(SystemOptionNames.PasswordNoneRepeatCount) ?? 0;
        if (noneRepeatCount > 0)
        {
            var prevUserPasswords = await _database.UserSecrets
                .AsNoTracking()
                .Where(e => e.UserId == id)
                .OrderByDescending(e => e.Create)
                .Select(e => e.Value)
                .Take(noneRepeatCount)
                .ToArrayAsync();

            if (prevUserPasswords.Any(o => _encryptService.Decrypt(o) == newPassword))
            {
                throw new Exception($"新密碼請勿與前 {noneRepeatCount} 次密碼相同！請輸入一個前 {noneRepeatCount} 次沒有用過的新密碼。");
            }
        }

        return true;
    }

    public virtual async Task<bool> CheckPasswordAsync(Guid id, string password, string newPassword, string confirm)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new Exception("舊密碼不可空白！");
        }

        if (string.IsNullOrEmpty(newPassword))
        {
            throw new Exception("新密碼不可空白！");
        }

        if (newPassword != confirm)
        {
            throw new Exception("新密碼與確認密碼不相等！");
        }

        if (!IsValidPassword(newPassword))
        {
            throw new Exception("密碼安全性不足！至少需要八個字元，且包含大小寫英文、數字與特殊符號。");
        }

        var noneRepeatCount = await _systemOptionService.GetIntAsync(SystemOptionNames.PasswordNoneRepeatCount) ?? 0;

        var prevUserPasswords = noneRepeatCount > 0 ?
            await _database.UserSecrets
                .AsNoTracking()
                .Where(e => e.UserId == id)
                .OrderByDescending(e => e.Create)
                .Select(e => e.Value)
                .Take(noneRepeatCount)
                .ToArrayAsync() :
            await _database.UserSecrets
                .AsNoTracking()
                .Where(e => e.UserId == id)
                .OrderByDescending(e => e.Create)
                .Select(e => e.Value)
                .ToArrayAsync();

        var currentPassword = prevUserPasswords.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(currentPassword) && _encryptService.Decrypt(currentPassword) != password)
        {
            throw new Exception("舊密碼錯誤！");
        }

        if (noneRepeatCount > 0 && prevUserPasswords.Any(o => _encryptService.Decrypt(o) == newPassword))
        {
            throw new Exception($"新密碼請勿與前 {noneRepeatCount} 次密碼相同！請輸入一個前 {noneRepeatCount} 次沒有用過的新密碼。");
        }

        return true;
    }

    public async Task EnsureDefaultUserAsync()
    {
        if (await _database.Users.AnyAsync(e => e.Status != UserStatus.Removed))
        {
            return;
        }

        var systemAdminGroup = await _database.UserGroups
            .AsNoTracking()
            .Where(e => e.GroupType == PermissionType.SystemAdmin)
            .FirstOrDefaultAsync();

        var systemAdminGroupCreated = false;
        if (systemAdminGroup == null)
        {
            systemAdminGroup = new UserGroup
            {
                Name = "系統管理員",
                Description = "系統預設的系統管理員群組，此群組成員擁有系統內所有權限。",
                GroupType = PermissionType.SystemAdmin
            };
            _database.UserGroups.Add(systemAdminGroup);
            systemAdminGroupCreated = true;
        }

        var defaultUser = new User
        {
            Account = "madmin",
            Name = "預設系統管理員",
            Title = "系統管理員",
            Description = "此帳號為系統預設的系統管理員，請於正式管理員帳號建立後，人工將此帳號停用。",
            Password = _encryptService.Encrypt("123"),
            Status = UserStatus.Enabled,
            Secrets = new List<UserSecret>()
            {
                new UserSecret
                {
                    Value = _encryptService.Encrypt("123"),
                    ValidTo = DateTime.Today.AddMonths(12),
                    Reason = UserSecretCreateReason.SystemApply
                }
            },
            UserGroupLinks = new List<UserGroupLink>()
            {
                new UserGroupLink
                {
                    UserGroupId = systemAdminGroup.Id
                }
            }
        };

        _database.Users.Add(defaultUser);
        await _database.SaveChangesAsync();
        _database.Detach(defaultUser);

        if (systemAdminGroupCreated)
        {
            _database.Detach(systemAdminGroup);
        }
    }

    protected virtual async Task<User> GetEntityAsync(Guid id)
    {
        return await _database.Users
            .Where(e => e.Id == id && e.Status != UserStatus.Removed)
            .Include(e => e.UserGroupLinks)
            .SingleOrDefaultAsync() ?? throw new Exception("找不到使用者");
    }

    protected virtual async Task<bool> CheckAndUpdatePasswordAsync(User entity, string newPassword, string confirm)
    {
        await CheckPasswordAsync(entity.Id, newPassword, confirm);

        AddInfo(entity.Account, nameof(entity.Password));
        entity.Password = _encryptService.Encrypt(newPassword);

        _database.UserSecrets.Add(new UserSecret
        {
            UserId = entity.Id,
            Value = _encryptService.Encrypt(newPassword),
            Reason = UserSecretCreateReason.UserChange
        });

        return true;
    }

    protected virtual async Task<bool> CheckAndUpdatePasswordAsync(User entity, string password, string newPassword, string confirm)
    {
        await CheckPasswordAsync(entity.Id, password, newPassword, confirm);

        AddInfo(entity.Account, nameof(entity.Password));
        entity.Password = _encryptService.Encrypt(newPassword);

        _database.UserSecrets.Add(new UserSecret
        {
            UserId = entity.Id,
            Value = _encryptService.Encrypt(newPassword),
            Reason = UserSecretCreateReason.UserChange
        });

        return true;
    }

    protected virtual void AddInfo(string target, string field, string before = "", string after = "")
    {
        _logService.SystemInfo("", target, field, before, after);
    }

    protected virtual bool Equals(string? textA, string? textB, bool ignoreCase = false)
    {
        return textA == null && textB == null || textA != null && textB != null && textA.Equals(textB, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
    }
}