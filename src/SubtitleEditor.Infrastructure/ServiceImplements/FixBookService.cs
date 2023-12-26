using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Database;
using SubtitleEditor.Database.Entities;
using SubtitleEditor.Infrastructure.Models.FixBook;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class FixBookService : IFixBookService
{
    private readonly EditorContext _database;
    private readonly ILogService _logService;

    public FixBookService(
        EditorContext database,
        ILogService logService
        )
    {
        _database = database;
        _logService = logService;
    }

    public async Task<FixBookData[]> ListAsync()
    {
        var fixBooks = await _database.FixBooks
            .AsNoTracking()
            .Where(e => e.Model != null)
            .ToArrayAsync();

        return fixBooks
            .GroupBy(e => e.Model!)
            .Select(g => new FixBookData
            {
                ModelName = g.Key,
                Items = g
                    .Select(e => new FixBookItem
                    {
                        Original = e.Original,
                        Correction = e.Correction
                    })
                .ToArray()
            })
            .ToArray();
    }

    public Task<FixBookData> GetByModelAsync()
    {
        return GetByModelAsync("Default");
    }

    public async Task<FixBookData> GetByModelAsync(string modelName)
    {
        _logService.Target = modelName;

        var fixBooks = await _database.FixBooks
            .AsNoTracking()
            .Where(e => e.Model == modelName)
            .ToArrayAsync();

        return new FixBookData
        {
            ModelName = modelName,
            Items = fixBooks
                .Select(e => new FixBookItem
                {
                    Original = e.Original,
                    Correction = e.Correction
                })
            .ToArray()
        };
    }

    public async Task SaveAsync(FixBookData data)
    {
        var exists = await _database.FixBooks
            .AsNoTracking()
            .Where(e => e.Model == data.ModelName)
            .ToArrayAsync();

        foreach (var e in exists)
        {
            _database.FixBooks.Remove(e);
        }

        foreach (var item in data.Items)
        {
            _database.FixBooks.Add(new FixBook
            {
                Model = data.ModelName,
                Original = item.Original,
                Correction = item.Correction
            });
        }

        await _database.SaveChangesAsync();
    }
}
