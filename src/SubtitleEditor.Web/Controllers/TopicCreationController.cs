using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Web.ActionFilters;
using SubtitleEditor.Web.Infrastructure.Models.Topic;
using SubtitleEditor.Web.Infrastructure.Services;

namespace SubtitleEditor.Web.Controllers;

public class TopicCreationController : AuthorizedController
{
    private readonly ITopicService _topicService;

    public TopicCreationController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    [HttpGet]
    [ViewWithLog(SystemAction.CreateTopics, OnlyLogOnError = true)]
    public IActionResult Batch()
    {
        return View();
    }

    [HttpPost]
    [ActionWithLog(SystemAction.CreateTopics)]
    public async Task<IActionResult> BatchCreation([FromBody] TopicCreationModel model)
    {
        var checkResult = _topicService.CheckForCreation(model);
        if (!checkResult.Success)
        {
            return From(checkResult);
        }

        await _topicService.CreateAsync(model);

        return Ok();
    }
}
