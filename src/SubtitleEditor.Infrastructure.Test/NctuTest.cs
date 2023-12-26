using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubtitleEditor.Infrastructure.ServiceImplements;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Infrastructure.Test;

[TestClass]
public class NctuTest
{
    private readonly IAsrService _asrService;

    public NctuTest()
    {
        var testConfig = new Dictionary<string, string>
        {
            { "ASR:Url", "https://61.219.178.73:8451"},
            { "ASR:User", "gbm_admin"},
            { "ASR:Secret", "Gbm@2021"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(testConfig)
            .Build();

        var services = new ServiceCollection();
        services.AddHttpClient("SkipCertificate").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IApiService, ApiService>();
        services.AddSingleton<IAsrService, AsrService>();

        var serviceProvider = services.BuildServiceProvider();
        _asrService = serviceProvider.GetRequiredService<IAsrService>();
    }

    [TestMethod]
    public async Task TestListModel()
    {
        var models = await _asrService.ListModelAsync();
        Assert.IsTrue(models != null && models.Any());
    }

    [TestMethod]
    public async Task TestListTask()
    {
        var listResult = await _asrService.ListTaskAsync(new Models.Asr.NctuListTaskRequest()
        {
            PageIndex = 0,
            PageSize = 10
        });

        Assert.IsTrue(listResult != null && listResult.Length > 0);
    }

    [TestMethod]
    public async Task TestGetTaskTotalCount()
    {
        var count = await _asrService.GetTaskTotalCountAsync(new Models.Asr.NctuListTaskRequest());

        Assert.IsTrue(count > 0);
    }

    [TestMethod]
    public async Task TestGetTask()
    {
        var nctuTask = await _asrService.GetTaskAsync(66);
        Assert.IsTrue(nctuTask != null && nctuTask.Id == 66);
    }

    [TestMethod]
    public async Task TestGetSubtitleLink()
    {
        var map = await _asrService.GetSubtitleLinkAsync(66);
        Assert.IsTrue(map != null && map.Any());
    }

    [TestMethod]
    public async Task TestGetFileLink()
    {
        var url = await _asrService.GetTaskFileLinkAsync(66);
        Assert.IsTrue(!string.IsNullOrEmpty(url));
    }

    [TestMethod]
    public async Task TestGetTranscriptLink()
    {
        var url = await _asrService.GetTaskTranscriptLinkAsync(66);
        Assert.IsTrue(!string.IsNullOrEmpty(url));
    }

    [TestMethod]
    public async Task TestGetWordSegments()
    {
        var segments = await _asrService.GetTaskWordSegmentsAsync(66);
        Assert.IsTrue(segments != null && segments.Any());
    }

    [TestMethod]
    public async Task TestDownloadSubtitleLink()
    {
        var map = await _asrService.GetSubtitleLinkAsync(66);
        var results = new List<string>();

        foreach (var pair in map)
        {
            var result = await _asrService.RetrieveTextFileAsync(pair.Value);
            results.Add(result);
        }

        Assert.IsTrue(map.Count == results.Count && !results.Any(o => string.IsNullOrEmpty(o)));
    }
}