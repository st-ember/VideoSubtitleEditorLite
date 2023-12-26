using SubtitleEditor.Infrastructure.Models.Asr;
using SubtitleEditor.Infrastructure.Models.FixBook;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class NctuFixBookService : IFixBookService
{
    private readonly IAsrService _asrService;
    private readonly ILogService _logService;

    public NctuFixBookService(
        IAsrService asrService,
        ILogService logService
        )
    {
        _asrService = asrService;
        _logService = logService;
    }

    public async Task<FixBookData[]> ListAsync()
    {
        var nctuFixBooks = await _asrService.GetFixBookAsync();
        return nctuFixBooks
            .Select(m => new FixBookData
            {
                ModelName = m.ModelName,
                MaxFixbookSize = m.MaxFixbookSize,
                Items = m.Fixbook
                    .Select(o => new FixBookItem
                    {
                        Original = o.X,
                        Correction = o.O
                    })
                .ToArray()
            })
            .ToArray();
    }

    public async Task<FixBookData> GetByModelAsync()
    {
        var models = await _asrService.ListModelAsync();
        return await GetByModelAsync(models.First().Name);
    }

    public async Task<FixBookData> GetByModelAsync(string modelName)
    {
        _logService.Target = modelName;

        var nctuFixBooks = await _asrService.GetFixBookAsync();
        var nctuFixBook = nctuFixBooks
            .Where(m => m.ModelName == modelName)
            .FirstOrDefault() ?? throw new Exception($"找不到名為 {modelName} 的勘誤表。");

        return new FixBookData
        {
            ModelName = modelName,
            MaxFixbookSize = nctuFixBook.MaxFixbookSize,
            Items = nctuFixBook.Fixbook
                .Select(m => new FixBookItem
                {
                    Original = m.X,
                    Correction = m.O
                })
                .ToArray()
        };
    }

    public async Task SaveAsync(FixBookData data)
    {
        _logService.Target = data.ModelName;

        var id = 0;
        await _asrService.SaveFixBookAsync(new NctuSaveFixBookRequest
        {
            ModelName = data.ModelName ?? "",
            Fixbook = data.Items
                .Select(m => new NctuFixBookItem
                {
                    Id = id++,
                    X = m.Original,
                    O = m.Correction
                })
                .ToArray()
        });
    }
}