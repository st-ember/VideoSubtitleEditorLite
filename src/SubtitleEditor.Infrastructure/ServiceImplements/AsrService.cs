using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Infrastructure.Models.Asr;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class AsrService : IAsrService
{
    private readonly IApiService _apiService;
    private readonly ISystemOptionService _systemOptionService;

    private string _token = "";
    private DateTime _tokenExpires = default;

    public AsrService(
        IApiService apiService,
        ISystemOptionService systemOptionService
        )
    {
        _apiService = apiService;
        _systemOptionService = systemOptionService;
    }

    private async Task<string> _getAsrUrlAsync()
    {
        return (await _systemOptionService.GetAsync(SystemOptionNames.AsrUrl))?.Content ?? string.Empty;
    }

    private async Task<string> _getAsrUserAsync()
    {
        return (await _systemOptionService.GetAsync(SystemOptionNames.AsrUser))?.Content ?? string.Empty;
    }

    private async Task<string> _getAsrSecretAsync()
    {
        return (await _systemOptionService.GetAsync(SystemOptionNames.AsrSecret))?.Content ?? string.Empty;
    }

    public async Task<NctuTask?> GetTaskAsync(long id, CancellationToken? cancellationToken = null)
    {
        await _ensureTokenAsync(cancellationToken);

        var request = new NctuListTaskRequest { Id = id };
        var list = await ListTaskAsync(request, cancellationToken);

        return list.FirstOrDefault();
    }

    public async Task<NctuTask[]> ListTaskAsync(NctuListTaskRequest request, CancellationToken? cancellationToken = null)
    {
        await _ensureTokenAsync(cancellationToken);

        var response = await _apiService.GetAsync<NctuListTaskResponse>($"{await _getAsrUrlAsync()}/api/v1/subtitle/tasks", request.ToQueryData(), cancellationToken);
        if (response?.Success ?? false)
        {
            return response.Data;
        }
        else
        {
            throw new Exception($"列出任務失敗：{response?.Error ?? ""}");
        }
    }

    public async Task<long> GetTaskTotalCountAsync(NctuListTaskRequest request, CancellationToken? cancellationToken = null)
    {
        await _ensureTokenAsync(cancellationToken);

        var response = await _apiService.GetAsync<NctuCountTaskResponse>($"{await _getAsrUrlAsync()}/api/v1/subtitle/tasks/total", request.ToQueryData(), cancellationToken);
        if (response?.Success ?? false)
        {
            return response.TotalCount;
        }
        else
        {
            throw new Exception($"取得任務總數失敗：{response?.Error ?? ""}");
        }
    }

    public async Task<string> GetTaskFileLinkAsync(long id, CancellationToken? cancellationToken = null)
    {
        await _ensureTokenAsync(cancellationToken);

        var response = await _apiService.GetAsync<NctuTaskFileLinkResponse>($"{await _getAsrUrlAsync()}/api/v1/subtitle/tasks/{id}/audio-link?type=source", cancellationToken);
        if (response.Success)
        {
            return response.Url;
        }
        else
        {
            throw new Exception($"取得任務原始檔案路徑失敗：{response.Error ?? ""}");
        }
    }

    public async Task<Dictionary<string, string>> GetSubtitleLinkAsync(long id, CancellationToken? cancellationToken = null)
    {
        await _ensureTokenAsync(cancellationToken);

        var response = await _apiService.GetAsync<NctuTaskSubtitleLinkResponse>($"{await _getAsrUrlAsync()}/api/v1/subtitle/tasks/{id}/subtitle-link", cancellationToken);
        if (response.Success)
        {
            return response.Urls;
        }
        else
        {
            throw new Exception($"取得字幕檔案路徑失敗：{response.Error ?? ""}");
        }
    }

    public async Task<string> GetTaskTranscriptLinkAsync(long id, CancellationToken? cancellationToken = null)
    {
        await _ensureTokenAsync(cancellationToken);

        var response = await _apiService.GetAsync<NctuTaskTranscriptLinkResponse>($"{await _getAsrUrlAsync()}/api/v1/subtitle/tasks/{id}/file_path?target=resultScriptFilePath", cancellationToken);
        if (response.Success)
        {
            return response.Url;
        }
        else
        {
            throw new Exception($"取得逐字稿檔案路徑失敗：{response.Error ?? ""}");
        }
    }

    public async Task<NctuWordSegment[]> GetTaskWordSegmentsAsync(long id, CancellationToken? cancellationToken = null)
    {
        await _ensureTokenAsync(cancellationToken);

        try
        {
            var response = await _apiService.GetAsync<NctuWordTimeResponse>($"{await _getAsrUrlAsync()}/api/v1/subtitle/tasks/{id}/file?target=resultWordTimeFilePath", cancellationToken);
            return response.Result;
        }
        catch (Exception ex)
        {
            throw new Exception($"取得字詞時間失敗：{ex}");
        }
    }

    public async Task<NctuASRModel[]> ListModelAsync(CancellationToken? cancellationToken = null)
    {
        await _ensureTokenAsync(cancellationToken);

        var response = await _apiService.GetAsync<NctuListModelResponse>($"{await _getAsrUrlAsync()}/api/v1/models", cancellationToken);
        if (response.Success)
        {
            return response.Data;
        }
        else
        {
            throw new Exception($"列出可用 model 失敗：{response.Error ?? ""}");
        }
    }

    public async Task<Stream> RetrieveFileAsync(string url, CancellationToken? cancellationToken = null)
    {
        return await _apiService.GetFileAsync($"{url}?token={System.Web.HttpUtility.UrlEncode(await _ensureTokenAsync())}", cancellationToken);
    }

    public async Task<string> RetrieveTextFileAsync(string url, CancellationToken? cancellationToken = null)
    {
        using var stream = await RetrieveFileAsync(url, cancellationToken);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public async Task<long> CreateTaskAsync(NctuCreateTaskRequest request, CancellationToken? cancellationToken = null)
    {
        var models = await ListModelAsync();
        if (!models.Any(m => m.ModelStatus == 1))
        {
            throw new Exception("沒有可用的 model");
        }

        if (request.Stream == null)
        {
            throw new Exception("傳送的檔案無效");
        }

        request.Stream.Position = 0;

        var model = !string.IsNullOrWhiteSpace(request.ModelName) ?
            models.Where(m => m.ModelStatus == 1 && m.Name == request.ModelName).FirstOrDefault() :
            null;

        await _ensureTokenAsync(cancellationToken);
        var title = request.Title.Length > 64 ? string.Concat(request.Title.Take(64), cancellationToken) : request.Title;
        var adoptedModel = model ?? (models.Any(m => m.IsDefaultModel == 1) ?
            models.Where(m => m.IsDefaultModel == 1).First() :
            models.First());

        var response = await _apiService.PostFormAsync<NctuCreateTaskResponse>(
            apiUrl: $"{await _getAsrUrlAsync()}/api/v1/subtitle/tasks",
            formContent: new MultipartFormDataContent
            {
                { new StreamContent(request.Stream!), "sourceWebLink", request.Filename },
                //{ new ByteArrayContent(), "sourceWebLink", request.Filename },
                { new StringContent(request.SourceType), "sourceType" },
                { new StringContent(title), "title" },
                { new StringContent(request.AudioChannel), "audioChannel" },
                { new StringContent(request.Description), "description" },
                { new StringContent(adoptedModel.Name), "modelName" },
                { new StringContent(adoptedModel.Version), "modelVersion" }
            }, cancellationToken);

        if (response.Success)
        {
            return response.Id;
        }
        else
        {
            throw new Exception($"建立任務失敗：{response.Error ?? ""}");
        }
    }

    public async Task DeleteTaskAsync(long id, CancellationToken? cancellationToken = null)
    {
        await _ensureTokenAsync(cancellationToken);
        var response = await _apiService.DeleteAsync<NctuResponseBase>(apiUrl: $"{await _getAsrUrlAsync()}/api/v1/subtitle/tasks/{id}", cancellationToken);

        if (response == null || !response.Success)
        {
            throw new Exception($"刪除任務失敗：{response?.Error ?? ""}");
        }
    }

    public async Task<NctuFixBookModel[]> GetFixBookAsync(CancellationToken? cancellationToken = null)
    {
        await _ensureTokenAsync(cancellationToken);
        var response = await _apiService.GetAsync<NctuResponseBase<NctuFixBookModel>>(apiUrl: $"{await _getAsrUrlAsync()}/api/v1/fixbook/text", cancellationToken);
        if (response.Success)
        {
            return response.Data;
        }
        else
        {
            throw new Exception($"列出勘誤表失敗：{response.Error ?? ""}");
        }
    }

    public async Task SaveFixBookAsync(NctuSaveFixBookRequest request, CancellationToken? cancellationToken = null)
    {
        await _ensureTokenAsync(cancellationToken);
        var response = await _apiService.PostAsync<NctuResponseBase>(apiUrl: $"{await _getAsrUrlAsync()}/api/v1/fixbook/text", request, cancellationToken);
        if (response == null || !response.Success)
        {
            throw new Exception($"儲存勘誤表失敗：{response?.Error ?? ""}");
        }
    }

    public async Task<NctuAsrVersion> GetAsrVersionAsync(CancellationToken? cancellationToken = null)
    {
        await _ensureTokenAsync(cancellationToken);
        var response = await _apiService.GetAsync<NctuResponseBase<NctuAsrVersion>>(apiUrl: $"{await _getAsrUrlAsync()}/api/v1/status/version", cancellationToken);
        if (response.Success && response.Data.Any())
        {
            return response.Data.First();
        }
        else
        {
            throw new Exception($"取得 ASR 服務版本失敗：{response.Error ?? ""}");
        }
    }

    public async Task<NctuAsrServiceStatus> GetAsrServiceStatusAsync(CancellationToken? cancellationToken = null)
    {
        await _ensureTokenAsync(cancellationToken);
        var response = await _apiService.GetAsync<NctuResponseBase<NctuAsrServiceStatus>>(apiUrl: $"{await _getAsrUrlAsync()}/api/v1/status/service", cancellationToken);
        if (response.Success && response.Data.Any())
        {
            return response.Data.First();
        }
        else
        {
            throw new Exception($"取得 ASR 服務狀態失敗：{response.Error ?? ""}");
        }
    }

    private async Task<string> _ensureTokenAsync(CancellationToken? cancellationToken = null)
    {
        var token = await _getTokenAsync(cancellationToken);
        _apiService.SetToken(token);
        return token;
    }

    private async Task<string> _getTokenAsync(CancellationToken? cancellationToken = null)
    {
        if (string.IsNullOrWhiteSpace(_token) || _tokenExpires == default || _tokenExpires <= DateTime.Now.AddMinutes(10))
        {
            var response = await _apiService.PostAsync<NctuLoginResponse>($"{await _getAsrUrlAsync()}/api/v1/login", new { username = await _getAsrUserAsync(), password = await _getAsrSecretAsync() }, cancellationToken);
            if (response?.Success ?? false)
            {
                _token = response.Token;
                _tokenExpires = DateTime.Now.AddSeconds(response.Expiration);
            }
            else
            {
                throw new Exception($"登入 ASR 失敗：{response?.Error ?? ""}");
            }
        }

        return _token;
    }
}
