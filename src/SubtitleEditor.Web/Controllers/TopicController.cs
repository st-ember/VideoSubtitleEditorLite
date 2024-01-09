using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Services;
using SubtitleEditor.Web.ActionFilters;
using SubtitleEditor.Web.Infrastructure.ListAdeptors;
using SubtitleEditor.Web.Infrastructure.Models.Topic;
using SubtitleEditor.Web.Infrastructure.Services;
using SubtitleEditor.Web.Models.Topic;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace SubtitleEditor.Web.Controllers;

public class TopicController : AuthorizedController
{
    private readonly IGenericListService<TopicListProcessor> _listService;
    private readonly ILogService _logService;
    private readonly IFileService _fileService;
    private readonly ITopicService _topicService;
    private readonly ISubtitleService _subtitleService;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IActivationService _activationService;
    private readonly ISystemOptionService _systemOptionService;

    public TopicController(
        IGenericListService<TopicListProcessor> listService,
        ILogService logService,
        IFileService fileService,
        ITopicService topicService,
        ISubtitleService subtitleService,
        IBenchmarkService benchmarkService,
        IActivationService activationService,
        ISystemOptionService systemOptionService

        )
    {
        _listService = listService;
        _logService = logService;
        _fileService = fileService;
        _topicService = topicService;
        _subtitleService = subtitleService;
        _benchmarkService = benchmarkService;
        _activationService = activationService;
        _systemOptionService = systemOptionService;
    }

    [HttpGet]
    public IActionResult Entry()
    {
        return RedirectToAction("List");
    }

    [HttpGet]
    [ViewWithLog(SystemAction.ShowTopicListView, OnlyLogOnError = true)]
    public async Task<IActionResult> List(TopicListViewModel model)
    {
        if (string.IsNullOrEmpty(model.Start) && string.IsNullOrEmpty(model.End))
        {
            model.Start = DateTime.Today.AddMonths(-6).ToString("yyyy/MM/dd");
            model.End = DateTime.Today.ToString("yyyy/MM/dd");
        }
        model.AsrAccess = (await _activationService.CheckAsrAccessBeta()) ?? false;
        return View(model);
    }

    [HttpGet]
    [ActionWithLog(SystemAction.GetTopic, OnlyLogOnError = true)]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok(await _topicService.GetAsync(id));
    }

    [HttpGet]
    [ActionWithLog(SystemAction.GetTopicListItem, OnlyLogOnError = true)]
    public async Task<IActionResult> GetListItem(Guid id)
    {
        return Ok(await _topicService.GetTopicListItemAsync(id));
    }

    [HttpGet]
    [ActionWithLog(SystemAction.GetTopicSubtitle, OnlyLogOnError = true)]
    public async Task<IActionResult> GetSubtitle(Guid id)
    {
        return Ok(await _topicService.GetSubtitleAsync(id));
    }

    [HttpGet]
    [ActionWithLog(SystemAction.ListTopic, OnlyLogOnError = true)]
    public async Task<IActionResult> ListTopic(TopicListViewModel model)
    {
        await _listService.QueryAsync<TopicListViewModel, TopicListData>(model);
        return Ok(TopicListResponse.From(model));
    }

    [HttpGet]
    public async Task<IActionResult> GetAsrAccess()
    {
        var key = await _systemOptionService.GetContentAsync(SystemOptionNames.ActivationKey);
        var activationData = _activationService.ResolveKey(key);
        var asrAccess = _activationService.CheckAsrAccess(activationData);
        return Ok(asrAccess);
    }

    //[HttpGet]
    //public async Task<IActionResult> SetAsrAccess(TopicListViewModel model)
    //{
    //    var key = await _systemOptionService.GetContentAsync(SystemOptionNames.ActivationKey);
    //    var activationData = _activationService.ResolveKey(key);
    //    var asrAccess = _activationService.CheckAsrAccess(activationData);

    //    if (asrAccess.Message == "Full")
    //    {
    //        model.AsrAccess = true;
    //    }
    //    if (asrAccess.Message == "Limited")
    //    {
    //        model.AsrAccess = true;
    //    }
    //    else
    //    {
    //        model.AsrAccess = true;
    //    }

    //    return View(model);
    //}

    [HttpPost]
    [ActionWithLog(SystemAction.CreateTopic)]
    public async Task<IActionResult> Create([FromBody] TopicCreationModel model)
    {
        var checkResult = _topicService.CheckForCreation(model);
        if (!checkResult.Success)
        {
            return From(checkResult);
        }

        await _topicService.CreateAsync(model);

        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.UpdateTopic)]
    public async Task<IActionResult> Update([FromBody] TopicUpdateModel model)
    {
        var checkResult = _topicService.CheckForUpdate(model);
        if (!checkResult.Success)
        {
            return Ok(checkResult);
        }

        await _topicService.UpdateAsync(model);

        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.UpdateTopicWordLimit)]
    public async Task<IActionResult> UpdateWordLimit([FromBody] TopicUpdateWordLimitModel model)
    {
        await _topicService.UpdateWordLimitAsync(model.Id, model.WordLimit);
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.UpdateTopicSubtitle)]
    public async Task<IActionResult> UpdateSubtitle([FromBody] TopicUpdateSubtitleModel model)
    {
        await _topicService.UpdateSubtitleAsync(model);
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.UpdateTopicTranscript)]
    public async Task<IActionResult> UpdateTranscript([FromBody] TopicUpdateTranscriptModel model)
    {
        await _topicService.UpdateTranscriptAsync(model);
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.PauseTopic)]
    public async Task<IActionResult> Pause([FromBody] TopicIdModel model)
    {
        await _topicService.PauseAsync(new Guid[] { model.Id });
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.ResumeTopic)]
    public async Task<IActionResult> Resume([FromBody] TopicIdModel model)
    {
        await _topicService.ResumeAsync(new Guid[] { model.Id });
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.ReExecuteTopic)]
    public async Task<IActionResult> ReExecute([FromBody] TopicIdModel model)
    {
        await _topicService.ReExecuteAsync(new Guid[] { model.Id });
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.ArchiveTopic)]
    public async Task<IActionResult> SetNormal([FromBody] TopicIdModel model)
    {
        await _topicService.SetNormalAsync(new Guid[] { model.Id });
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.ArchiveTopic)]
    public async Task<IActionResult> Archive([FromBody] TopicIdModel model)
    {
        await _topicService.ArchiveAsync(new Guid[] { model.Id });
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.RemoveTopic)]
    public async Task<IActionResult> Remove([FromBody] TopicIdModel model)
    {
        await _topicService.RemoveAsync(new Guid[] { model.Id });
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.ArchiveTopic)]
    public async Task<IActionResult> BatchArchive([FromBody] TopicIdsModel model)
    {
        await _topicService.ArchiveAsync(model.Ids);
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.RemoveTopic)]
    public async Task<IActionResult> BatchRemove([FromBody] TopicIdsModel model)
    {
        await _topicService.RemoveAsync(model.Ids);
        return Ok();
    }

    [HttpGet]
    [ActionWithLog(SystemAction.ExportTranscript, OnlyLogOnError = true)]
    public async Task<IActionResult> ExportTranscript(Guid id, string? encoding)
    {
        var transcript = await _topicService.GetTranscriptAsync(id);
        var data = await _fileService.ConvertTextToFileDataAsync(transcript ?? "", _adjustEncoding(encoding));
        var ticket = _fileService.SaveToCache(data, "");
        return Ok(data: ticket);
    }

    [HttpGet]
    [ActionWithLog(SystemAction.ExportSubtitle, OnlyLogOnError = true)]
    public async Task<IActionResult> ExportSubtitle(Guid id, string format, string? encoding)
    {
        var subtitleData = await _topicService.GetSubtitleAsync(id);
        var textData = format.ToLower().Trim() switch
        {
            "srt" => _subtitleService.ToSrt(subtitleData.Subtitle),
            "vtt" => _subtitleService.ToVtt(subtitleData.Subtitle),
            "inline" => _subtitleService.ToInline(subtitleData.Subtitle, subtitleData.FrameRate),
            "notime" => _subtitleService.ToNoTime(subtitleData.Subtitle),
            _ => _subtitleService.ToSrt(subtitleData.Subtitle),
        };

        var data = await _fileService.ConvertTextToFileDataAsync(textData, _adjustEncoding(encoding));
        var ticket = _fileService.SaveToCache(data, "");
        return Ok(data: ticket);
    }

    [HttpGet]
    public async Task<IActionResult> DownloadRawFile(Guid id, string filename)
    {
        var result = await _logService.StartAsync(SystemAction.DownloadRawFile.ToString(), async () =>
        {
            return await _topicService.GetRawFileAsync(id);
        });

        if (result.Success && result.Data != null)
        {
            return File(result.Data, MediaTypeNames.Application.Octet, HttpUtility.UrlDecode(filename ?? "File"), true);
        }

        return StatusCode(400);
    }

    [HttpPost]
    [ActionWithLog(SystemAction.ReuploadSubtitle)]
    public async Task<IActionResult> ReuploadSubtitle([FromBody] ReuploadSubtitleModel model)
    {
        await _topicService.ReplaceSubtitleAsync(model.Id, model.Ticket, model.FrameRate);
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.ReuploadTranscript)]
    public async Task<IActionResult> ReuploadTranscript([FromBody] ReuploadTranscriptModel model)
    {
        await _topicService.ReplaceTranscriptAsync(model.Id, model.Ticket);
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.ReproduceSubtitle)]
    public async Task<IActionResult> ReproduceSubtitle([FromBody] TopicIdModel model)
    {
        await _topicService.ReproduceSubtitleAsync(new Guid[] { model.Id });
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.RecoverToOriginal)]
    public async Task<IActionResult> RecoverToOriginal([FromBody] TopicIdModel model)
    {
        await _topicService.RecoverToOriginalAsync(new Guid[] { model.Id });
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.ReproduceSubtitle)]
    public async Task<IActionResult> ReloadSubtitle([FromBody] TopicIdModel model)
    {
        await _topicService.ReloadSubtitleAsync(new Guid[] { model.Id });
        return Ok();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.DoTopicConversionBenchmark)]
    public async Task<IActionResult> DoTopicConversionBenchmark([FromBody] TopicBenchmarkModel model)
    {
        var result = await _benchmarkService.DoBenchmarkAsync(model.Id, model.ArgumentTemplate);
        return Ok(new
        {
            result.Success,
            result.Output,
            result.ArgumentTemplate,
            result.TransferCost,
            result.ConvertCost,
            result.Length,
            Start = result.Start.ToString("yyyy/MM/dd HH:mm:ss.fffffff"),
            PullRawFileFromAsr = result.PullRawFileFromAsr?.ToString("yyyy/MM/dd HH:mm:ss.fffffff"),
            PullRawFileFromLocal = result.PullRawFileFromLocal?.ToString("yyyy/MM/dd HH:mm:ss.fffffff"),
            SavedRawFile = result.SavedRawFile?.ToString("yyyy/MM/dd HH:mm:ss.fffffff"),
            StartedConvert = result.StartedConvert?.ToString("yyyy/MM/dd HH:mm:ss.fffffff"),
            CompletedConvert = result.CompletedConvert?.ToString("yyyy/MM/dd HH:mm:ss.fffffff"),
        });
    }

    private static Encoding _adjustEncoding(string? encoding)
    {
        return encoding switch
        {
            "UTF-16" => Encoding.Unicode,
            "UTF-8" => new UTF8Encoding(false),
            "UTF-8 with BOM" => new UTF8Encoding(true),
            _ => Encoding.Unicode
        };
    }
}
