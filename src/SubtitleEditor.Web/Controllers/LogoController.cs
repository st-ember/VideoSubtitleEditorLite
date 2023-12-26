using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Models;
using SubtitleEditor.Infrastructure.Services;
using System.Net.Mime;
using System.Text.Json;

namespace SubtitleEditor.Web.Controllers;

public class LogoController : Controller
{
    private readonly string _webImageFolder;
    private readonly ISystemOptionService _systemOptionService;
    private readonly IFileService _fileService;
    private readonly ICacheService _cacheService;

    private const string _logo_cache_key = "logo-cache";

    public LogoController(
        IWebHostEnvironment env,
        ISystemOptionService systemOptionService, 
        IFileService fileService,
        ICacheService cacheService
        )
    {
        _webImageFolder = Path.Combine(env.WebRootPath, "images");
        _systemOptionService = systemOptionService;
        _fileService = fileService;
        _cacheService = cacheService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var file = await _cacheService.GetOrCreateAsync(_logo_cache_key, TimeSpan.FromMinutes(1), async () =>
        {
            var siteLogo = await _systemOptionService.GetContentAsync(SystemOptionNames.LogoTicket) ?? "[]";
            var logoData = Array.Empty<byte>();
            var extension = string.Empty;
            if (!string.IsNullOrWhiteSpace(siteLogo))
            {
                var fileDatas = JsonSerializer.Deserialize<FileData[]>(siteLogo, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var ticket = fileDatas?.FirstOrDefault()?.Ticket;

                if (!string.IsNullOrWhiteSpace(ticket))
                {
                    logoData = await _fileService.RetrieveFromStorageAsync(ticket) ?? Array.Empty<byte>();
                    extension = ticket.Split('.').Last();
                }
            }

            if (logoData.Length == 0)
            {
                var localLogoFilePath = Directory.EnumerateFiles(_webImageFolder)
                    .Where(o => Path.GetFileName(o).StartsWith("logo"))
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(localLogoFilePath))
                {
                    logoData = await System.IO.File.ReadAllBytesAsync(localLogoFilePath);
                    extension = localLogoFilePath.Split(".").Last();
                }
            }

            return new { logoData, extension };
        });

        return File(file.logoData, MediaTypeNames.Application.Octet, $"logo.{file.extension}");
    }
}
