using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Infrastructure.Models;
using SubtitleEditor.Infrastructure.Services;
using System.Net.Mime;
using System.Web;

namespace SubtitleEditor.Web.Controllers;

public class FileController : Controller
{
    private readonly IFileService _fileService;

    public FileController(
        IFileService fileService
        )
    {
        _fileService = fileService;
    }

    [HttpPost]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<JsonResult> Upload()
    {
        try
        {
            var files = Request.Form.Files;
            var tickets = new List<string>();

            foreach (var file in files)
            {
                var ticket = await _fileService.SaveToStorageAsync(file.OpenReadStream(), Path.GetExtension(file.FileName).Replace(".", ""));
                tickets.Add(ticket);
            }

            return Json(new { success = true, data = tickets });
        }
        catch (Exception ex)
        {
            return WebResult.Error(ex);
        }
    }

    [HttpPost]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<JsonResult> UploadToCache()
    {
        try
        {
            var files = Request.Form.Files;
            var tickets = new List<string>();

            foreach (var file in files)
            {
                var ticket = await _fileService.SaveToCacheAsync(file.OpenReadStream(), Path.GetExtension(file.FileName).Replace(".", ""));
                tickets.Add(ticket);
            }

            return Json(new { success = true, data = tickets });
        }
        catch (Exception ex)
        {
            return WebResult.Error(ex);
        }
    }

    //[HttpGet]
    //public IActionResult Check(string ticket)
    //{
    //    return Json(new { success = _fileService.ExistsInStorage(ticket) });
    //}

    [HttpGet]
    public IActionResult CheckFromCache(string ticket)
    {
        return Json(new { success = _fileService.ExistsInCache(ticket) });
    }

    //[HttpGet]
    //public async Task<IActionResult> Download(string ticket, string filename, bool deleteAfterDownload = false)
    //{
    //    var file = await _fileService.RetrieveFromStorageAsync(ticket);
    //    if (file != null && file.Length > 0 && deleteAfterDownload)
    //    {
    //        await _fileService.DeleteFromStorageAsync(ticket);
    //    }

    //    if (file == null || file.Length == 0)
    //    {
    //        return NotFound();
    //    }

    //    return File(file, MediaTypeNames.Application.Octet, HttpUtility.UrlDecode(filename ?? "File"));
    //}

    [HttpGet]
    public async Task<IActionResult> DownloadFromCache(string ticket, string filename, bool deleteAfterDownload = false)
    {
        var stream = await _fileService.ReadFromCacheAsync(ticket);
        if (stream != null && stream.Length > 0 && deleteAfterDownload)
        {
            _fileService.DeleteCache(ticket);
        }

        if (stream == null || stream.Length == 0)
        {
            return NotFound();
        }

        return File(stream, MediaTypeNames.Application.Octet, HttpUtility.UrlDecode(filename ?? "File"));
    }

    //[HttpPost]
    //public async Task<IActionResult> Delete(string ticket)
    //{
    //    await _fileService.DeleteFromStorageAsync(ticket);
    //    return WebResult.Ok();
    //}

    //[HttpPost]
    //public IActionResult DeleteFromCache(string ticket)
    //{
    //    _fileService.DeleteCache(ticket);
    //    return WebResult.Ok();
    //}
}
