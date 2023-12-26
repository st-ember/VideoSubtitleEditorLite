using SubtitleEditor.Core.Models;

namespace SubtitleEditor.Infrastructure.Services;

public interface IActivationService
{
    Task<ActivationData?> GetActivationDataAsync();
    Task<uint?> GetCalCountAsync();
    Task<bool> IsActivatedAsync();
    ISimpleResult CheckActivationData(ActivationData activationData);
    Task SetActivationDataAsync(string key);
    Task ClearActivationDataAsync();
    ISimpleResult CheckAsrAccess(ActivationData activationData);
    ActivationData? ResolveKey(string? key);

    void ClearCache();
}
