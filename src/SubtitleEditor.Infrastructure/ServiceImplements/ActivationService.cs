using RSAExtensions;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Infrastructure.Models.SystemOption;
using SubtitleEditor.Infrastructure.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class ActivationService : IActivationService
{
    private readonly ISystemOptionService _systemOptionService;
    private readonly ICacheService _cacheService;
    private readonly IEncryptService _encryptService;

    private const string _calCountCacheKey = "activation-cal-count";
    private const string _activatedCacheKey = "activation-activated";

    private static string _publicKey => $"<RSAKeyValue><Modulus>{ActivationKeys.Publish}</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

    public ActivationService(
        ISystemOptionService systemOptionService,
        ICacheService cacheService,
        IEncryptService encryptService
        )
    {
        _systemOptionService = systemOptionService;
        _cacheService = cacheService;
        _encryptService = encryptService;
    }

    public async Task<ActivationData?> GetActivationDataAsync()
    {
        var key = await _systemOptionService.GetContentAsync(SystemOptionNames.ActivationKey);
        return ResolveKey(key);
    }

    public async Task<uint?> GetCalCountAsync()
    {
        return await _cacheService.GetOrCreateAsync(_calCountCacheKey, TimeSpan.FromHours(12), async () =>
        {
            var activationData = await GetActivationDataAsync();
            return activationData != null && (!activationData.DueDate.HasValue || activationData.DueDate.Value > DateTime.Now) ? (uint?)activationData.CalCount : null;
        });
    }

    public async Task<bool> IsActivatedAsync()
    {
        return await _cacheService.GetOrCreateAsync(_activatedCacheKey, TimeSpan.FromHours(12), async () =>
        {
            var activationData = await GetActivationDataAsync();
            return activationData != null && (!activationData.DueDate.HasValue || activationData.DueDate.Value > DateTime.Now);
        });
    }

    public ISimpleResult CheckActivationData(ActivationData activationData)
    {

        if (activationData.DueDate.HasValue && activationData.DueDate.Value < DateTime.Now)
        {
            return SimpleResult.IsFailed("金鑰已過期。");
        }

        if (activationData.PublishDate == default || activationData.DueDate.HasValue && activationData.DueDate.Value < activationData.PublishDate)
        {
            return SimpleResult.IsFailed("金鑰日期無效。");
        }

        if (string.IsNullOrEmpty(activationData.Publisher))
        {
            return SimpleResult.IsFailed("金鑰發行者無效。");
        }
        // 增加版本判斷
        // 原本通過上面三個判斷式即回傳success
        if (activationData.Editions == "Full" || activationData.Version == 1)
        {
            return SimpleResult.IsSuccess();
        }
        else if (activationData.Editions == "Limited")
        {
            return SimpleResult.IsSuccess("Limited");
        }
        return SimpleResult.IsFailed("金鑰發生未預期問題");
    }

    public ISimpleResult CheckAsrAccess(ActivationData activationData)
    {
        if(activationData != null)
        {
            if (activationData.Version == 1)
            {
                return SimpleResult.IsSuccess("Full");
            }

            if (activationData.Editions == "Full")
            {
                return SimpleResult.IsSuccess("Full");
            }

            if (activationData.Editions == "Limited")
            {
                return SimpleResult.IsSuccess("Limited");
            }
        }

        return SimpleResult.IsFailed("ASR權限發生未預期的問題");
    }

    public async Task SetActivationDataAsync(string key)
    {
        await _systemOptionService.SetAsync(new SystemOptionModel
        {
            Name = SystemOptionNames.ActivationKey,
            Content = key,
            Description = "Activation Key"
        });

        ClearCache();
    }

    public async Task ClearActivationDataAsync()
    {
        await _systemOptionService.SetAsync(new SystemOptionModel
        {
            Name = SystemOptionNames.ActivationKey,
            Content = string.Empty,
            Description = "Activation Key"
        });

        ClearCache();
    }

    public ActivationData? ResolveKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key) || !key.Contains(":::"))
        {
            return null;
        }

        try
        {
            using var provider = new RSACryptoServiceProvider();
            provider.FromXmlString(_publicKey);

            var array = key.Split(":::");
            var encryptedData = array[0];
            var encryptedKey = array[1];

            var encryptKey = Encoding.Default.GetString(provider.PublicDecryption(Convert.FromBase64String(encryptedKey)));
            var rawKey = _encryptService.Decrypt(encryptedData, encryptKey);

            return _deserializeActivationData(rawKey);
        }
        catch
        {
            return null;
        }
    }

    public void ClearCache()
    {
        _cacheService.Remove(_calCountCacheKey);
        _cacheService.Remove(_activatedCacheKey);
    }

    private static ActivationData? _deserializeActivationData(string key)
    {
        return !string.IsNullOrWhiteSpace(key) ? JsonSerializer.Deserialize<ActivationData>(key) : null;
    }
}
