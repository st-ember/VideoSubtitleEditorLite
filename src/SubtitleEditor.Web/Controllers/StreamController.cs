using Microsoft.AspNetCore.Mvc;
using SubtitleEditor.Infrastructure.Services;

namespace SubtitleEditor.Controllers;

public class StreamController : Controller
{
    private readonly string _streamFolder;
    private readonly IStreamFileService _streamFileService;

    public StreamController(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        IStreamFileService streamFileService
        )
    {
        _streamFolder = Path.Combine(environment.ContentRootPath, configuration["StorageFolder"], configuration["StreamFolder"]);
        _streamFileService = streamFileService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return StatusCode(StatusCodes.Status400BadRequest);
        }

        if (id.EndsWith(".ts"))
        {
            return _getTS(id);
        }
        else
        {
            return await _getM3U8Async(id);
        }
    }

    [HttpGet]
    public IActionResult Check(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Json(new { result = false, message = "輸入的資訊錯誤" });
        }

        var folderPath = Path.Combine(_streamFolder, id);
        if (!Directory.Exists(folderPath))
        {
            return Json(new { result = false, message = "找不到指定的多媒體物件" });
        }

        var m3u8FilePath = Path.Combine(folderPath, $"{id}.m3u8");
        if (!System.IO.File.Exists(m3u8FilePath))
        {
            return Json(new { result = false, message = "找不到指定的物件" });
        }

        return Json(new { result = true });
    }

    private async Task<IActionResult> _getM3U8Async(string id)
    {
        var filenameArray = id.Split('.');
        var streamId = filenameArray.Length > 1 ? id.Replace($".{filenameArray.Last()}", "") : id;

        var result = await _streamFileService.RetrieveM3U8Async(streamId);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result.Replace("media_", $"{streamId}_"));

        //var folderPath = Path.Combine(_streamFolder, id);
        //if (!Directory.Exists(folderPath))
        //{
        //    return NotFound();
        //}

        //var m3u8FilePath = Path.Combine(folderPath, $"{id}.m3u8");
        //if (!System.IO.File.Exists(m3u8FilePath))
        //{
        //    return NotFound();
        //}

        //var htsRoot = System.IO.File.ReadAllText(m3u8FilePath);
        //return Ok(htsRoot);
    }

    private IActionResult _getTS(string id)
    {
        var array = id.Split('_');
        var m3u8Id = array.First();
        var number = array.Length > 1 ? array[1].Split('.').First() : "";

        var stream = _streamFileService.RetrieveTsFile(m3u8Id, number);
        if (stream == null)
        {
            return NotFound();
        }

        return File(stream, "video/MP2T");

        //var folderPath = Path.Combine(_streamFolder, m3u8Id);
        //if (!Directory.Exists(folderPath))
        //{
        //    return NotFound();
        //}

        //var filePath = Path.Combine(folderPath, id);
        //if (!System.IO.File.Exists(filePath))
        //{
        //    return NotFound();
        //}

        //return File(System.IO.File.OpenRead(filePath), "video/MP2T");
    }
}
