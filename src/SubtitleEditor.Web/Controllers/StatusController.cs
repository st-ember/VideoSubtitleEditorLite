using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Models.Asr;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.ActionFilters;
using SubtitleEditor.Web.Infrastructure.Services;
using SubtitleEditor.Web.Models.Status;

namespace SubtitleEditor.Web.Controllers;

public class StatusController : AuthorizedController
{
    private readonly string _version;
    private readonly ISystemOptionService _systemOptionService;
    private readonly IAsrService _asrService;
    private readonly ITopicService _topicService;
    private readonly IActivationService _activationService;

    public StatusController(
        IConfiguration configuration,
        IAsrService asrService,
        ISystemOptionService systemOptionService,
        ITopicService topicService,
        IActivationService activationService
        )
    {
        _version = configuration["Version"];
        _asrService = asrService;
        _systemOptionService = systemOptionService;
        _topicService = topicService;
        _activationService = activationService;
    }

    [HttpGet]
    [ViewWithLog(SystemAction.ShowSystemStatusView, OnlyLogOnError = true)]
    public async Task<IActionResult> Index(SystemStatusModel model)
    {
        var key = await _systemOptionService.GetContentAsync(SystemOptionNames.ActivationKey);
        var activationData = _activationService.ResolveKey(key);
        var asrAccess = _activationService.CheckAsrAccess(activationData);

        if (asrAccess.Message == "Full")
        {
            model.AsrAccess = true;
        }
        else if (asrAccess.Message == "Limited")
        {
            model.AsrAccess = false;
        }
        else
        {
            model.AsrAccess = false;
        }
        return View(model);
    }

    [HttpGet]
    [ActionWithLog(SystemAction.GetSystemStatus, OnlyLogOnError = true)]
    public async Task<IActionResult> GetStatus()
    {
        NctuAsrVersion? version = null;
        NctuAsrServiceStatus? asrStatus = null;

        try
        {
            version = await _asrService.GetAsrVersionAsync();
            asrStatus = await _asrService.GetAsrServiceStatusAsync();
        }
        catch { }
        
        var activationData = await _activationService.GetActivationDataAsync();

        return Ok(new SystemStatusModel
        {
            AsrKernelVersion = version?.AsrKernelVersion,
            CaptionMakerVersion = version?.CaptionMakerVersion,
            VideoSubtitleEditorVersion = _version,
            TotalWorkers = asrStatus?.TotalWorkers,
            AvailableWorkers = asrStatus?.AvailableWorkers,
            AsrStatus = asrStatus?.Status,
            LicenseExpiredTime = asrStatus != null ? (DateTime.TryParse(asrStatus.LicenseExpiredTime, out var t) ? t : default).ToString("yyyy/MM/dd HH:mm:ss") : null,

            StorageLimit = await _systemOptionService.GetLongAsync(SystemOptionNames.RawFileStorageLimit) ?? 0,
            StreamFileLimit = await _systemOptionService.GetLongAsync(SystemOptionNames.StreamFileStorageLimit) ?? 0,
            StorageLength = await _topicService.GetTotalRawFileSizeAsync(),
            StreamFileLength = await _topicService.GetTotalStreamFileSizeAsync(),

            Activated = activationData != null && (!activationData.DueDate.HasValue || activationData.DueDate.Value > DateTime.Now),
            ActivationKeyPublisher = activationData?.Publisher ?? string.Empty,
            ActivatedTarget = activationData?.Target ?? string.Empty,
            CalCount = activationData?.CalCount,
            ActivationEnd = activationData?.Due ?? string.Empty,
        });
    }

    [HttpPost]
    [ActionWithLog(SystemAction.SetActivationKey)]
    public async Task<IActionResult> SetActivationKey([FromBody] SetActivationKeyModel model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Key))
        {
            ThrowArgumentError("提供的金鑰錯誤。");
        }

        var activationData = _activationService.ResolveKey(model!.Key) ?? throw new Exception("無效的金鑰。");
        var checkResult = _activationService.CheckActivationData(activationData);
        if (!checkResult.Success)
        {
            return From(checkResult);
        }

        await _activationService.SetActivationDataAsync(model!.Key);

        return Ok(checkResult);
    }

    [HttpPost]
    [HttpGet]
    [ActionWithLog(SystemAction.ClearActivationKey)]
    public async Task<IActionResult> ClearActivationKey()
    {
        await _activationService.ClearActivationDataAsync();
        return Ok();
    }
}