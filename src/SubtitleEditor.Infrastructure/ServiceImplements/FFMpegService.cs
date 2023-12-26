using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SubtitleEditor.Infrastructure.Models.FFMpeg;
using SubtitleEditor.Infrastructure.Services;
using System.Diagnostics;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class FFMpegService : IFFMpegService
{
    private readonly ILogger _logger;
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;
    private readonly string _argumentTemplate;
    private readonly IFileService _fileService;

    private bool _windowsVersion = false;
    private const string _linuxFFMpegPath = "/app/ffmpeg";
    private const string _linuxFFProbePath = "/app/ffprobe";
    private const string _fallbackFFMpegPath = "ffmpeg";
    private const string _fallbackFFProbePath = "ffprobe";

    public FFMpegService(
        ILogger<FFMpegService> logger,
        IWebHostEnvironment env,
        IConfiguration configuration,
        IFileService fileService
        )
    {
        _logger = logger;

        var rootPath = env.ContentRootPath;
        _ffmpegPath = Path.Combine(rootPath, configuration["StreamConverter:FFMpegFolder"], configuration["StreamConverter:FFMpegPath"]);
        _ffprobePath = Path.Combine(rootPath, configuration["StreamConverter:FFMpegFolder"], configuration["StreamConverter:FFProbePath"]);
        _argumentTemplate = configuration["StreamConverter:ArgumentTemplate"];
        _fileService = fileService;
    }

    public async Task DetermineFFMpegSourceAsync()
    {
        _windowsVersion = Environment.OSVersion.Platform == PlatformID.Win32NT;
        
        if (!_windowsVersion)
        {
            try
            {
                var adoptedFFMpegPath = File.Exists(_linuxFFMpegPath) ? _linuxFFMpegPath : _fallbackFFMpegPath;
                var ffmpegTestProcessInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = adoptedFFMpegPath,
                    Arguments = "-hide_banner -version"
                };

                var ffmpegTestProcess = Process.Start(ffmpegTestProcessInfo)!;
                var output = await ffmpegTestProcess.StandardOutput.ReadLineAsync();

                if (!string.IsNullOrWhiteSpace(output))
                {
                    _logger.LogInformation("ffmpeg test succeed with output= {output}", output);
                }
                else
                {
                    _logger.LogError("ffmpeg test failed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("ffmpeg test failed, error= {ex}", ex);
            }

            try
            {
                var adoptedFFProbePath = File.Exists(_linuxFFProbePath) ? _linuxFFProbePath : _fallbackFFProbePath;
                var ffprobeTestProcessInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = adoptedFFProbePath,
                    Arguments = "-version"
                };

                var ffprobeTestProcess = Process.Start(ffprobeTestProcessInfo)!;
                var output = await ffprobeTestProcess.StandardOutput.ReadLineAsync();

                if (!string.IsNullOrWhiteSpace(output))
                {
                    _logger.LogInformation("ffprobe test succeed with output= {output}", output);
                }
                else
                {
                    _logger.LogError("ffprobe test failed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("ffprobe test failed, error= {ex}", ex);
            }
        }
    }

    public async Task<string[]> ListHardwareAccelerationAsync()
    {
        var processStartInfo = new ProcessStartInfo()
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            FileName = "ffmpeg",
            Arguments = "-hide_banner -hwaccels"
        };

        var process = Process.Start(processStartInfo)!;
        process.WaitForExit();

        await Task.Delay(500);

        var output = process.StandardOutput.ReadToEnd();
        return output.Split('\n').Skip(1)
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .ToArray();
    }

    public Task<ConvertToM3U8Result> ConvertToM3U8Async(string sourceFilePath, CancellationToken stoppingToken = default)
    {
        return ConvertToM3U8Async(sourceFilePath, _argumentTemplate, stoppingToken);
    }

    public async Task<ConvertToM3U8Result> ConvertToM3U8Async(string sourceFilePath, string argumentTemplate, CancellationToken stoppingToken = default)
    {
        var outputFolderPath = Path.Combine(_fileService.WorkspaceFolder, "output");
        var m3u8OutputPath = Path.Combine(outputFolderPath, "media_.m3u8");

        _checkSourceFile(sourceFilePath);

        if (Directory.Exists(outputFolderPath))
        {
            Directory.Delete(outputFolderPath, true);
        }

        Directory.CreateDirectory(outputFolderPath);

        if (_windowsVersion)
        {
            return await _convertToM3U8WindowsAsync(sourceFilePath, m3u8OutputPath, outputFolderPath, argumentTemplate, stoppingToken);
        }
        else
        {
            return await _convertToM3U8LinuxAsync(sourceFilePath, m3u8OutputPath, outputFolderPath, argumentTemplate, stoppingToken);
        }
    }

    public async Task<double> GetDurationAsync(string sourceFilePath, CancellationToken stoppingToken = default)
    {
        _checkSourceFile(sourceFilePath);

        if (_windowsVersion)
        {
            return await _getDurationWindowsAsync(sourceFilePath, stoppingToken);
        }
        else
        {
            return await _getDurationLinuxAsync(sourceFilePath, stoppingToken);
        }
    }

    private static async Task<ConvertToM3U8Result> _convertToM3U8LinuxAsync(string sourceFilePath, string m3u8OutputPath, string outputFolderPath, string argumentTemplate, CancellationToken stoppingToken = default)
    {
        var adoptedFFMpegPath = File.Exists(_linuxFFMpegPath) ? _linuxFFMpegPath : _fallbackFFMpegPath;
        var processStartInfo = new ProcessStartInfo()
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            FileName = adoptedFFMpegPath,
            Arguments = argumentTemplate.Replace("{sourceFilePath}", sourceFilePath).Replace("{outputPath}", m3u8OutputPath)
        };

        return await Task.Run(() =>
        {
            var outputLines = new List<string>();
            var process = new Process()
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };

            process.ErrorDataReceived += (o, e) =>
            {
                outputLines.Add(e.Data ?? "");
            };

            process.Start();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return new ConvertToM3U8Result
            {
                OutputFilePaths = Directory.GetFiles(outputFolderPath),
                Output = string.Join('\n', outputLines)
            };
        }, stoppingToken);
    }

    private async Task<ConvertToM3U8Result> _convertToM3U8WindowsAsync(string sourceFilePath, string m3u8OutputPath, string outputFolderPath, string argumentTemplate, CancellationToken stoppingToken = default)
    {
        var processStartInfo = new ProcessStartInfo()
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            FileName = _ffmpegPath,
            Arguments = argumentTemplate.Replace("{sourceFilePath}", sourceFilePath).Replace("{outputPath}", m3u8OutputPath)
        };

        return await Task.Run(() =>
        {
            var outputLines = new List<string>();
            var process = new Process()
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };

            process.ErrorDataReceived += (o, e) =>
            {
                outputLines.Add(e.Data ?? "");
            };

            process.Start();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return new ConvertToM3U8Result
            {
                OutputFilePaths = Directory.GetFiles(outputFolderPath),
                Output = string.Join('\n', outputLines)
            };
        }, stoppingToken);
    }

    private async Task<double> _getDurationLinuxAsync(string sourceFilePath, CancellationToken stoppingToken = default)
    {
        var adoptedFFProbePath = File.Exists(_linuxFFProbePath) ? _linuxFFProbePath : _fallbackFFProbePath;
        var processStartInfo = new ProcessStartInfo()
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            FileName = adoptedFFProbePath,
            Arguments = $"-i \"{sourceFilePath}\" -show_entries format=duration -v quiet"
        };

        return await Task.Run(async () =>
        {
            var process = Process.Start(processStartInfo)!;
            process.WaitForExit();

            var output = await process.StandardOutput.ReadToEndAsync();
            _logger.LogInformation("GetDurationAsync, output= {output}", output);

            var durationLine = output!.Split('\n').Where(o => o.StartsWith("duration=")).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(durationLine))
            {
                return 0;
            }

            var resultString = durationLine.Split('=').Last();
            var validDuration = double.TryParse(resultString, out var duration);
            return validDuration ? duration : 0;
        }, stoppingToken);
    }

    private async Task<double> _getDurationWindowsAsync(string sourceFilePath, CancellationToken stoppingToken = default)
    {
        var processStartInfo = new ProcessStartInfo()
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            FileName = _ffprobePath,
            Arguments = $"-i \"{sourceFilePath}\" -show_entries format=duration -v quiet"
        };

        return await Task.Run(async () =>
        {
            var process = Process.Start(processStartInfo)!;
            process.WaitForExit();

            await Task.Delay(500);

            var output = process.StandardOutput.ReadToEnd();
            var durationLine = output.Split('\n').Where(o => o.StartsWith("duration=")).FirstOrDefault();
            _logger.LogInformation("GetDurationAsync, output= {output}", output);

            if (string.IsNullOrWhiteSpace(durationLine))
            {
                return 0;
            }

            var resultString = durationLine.Split('=').Last();
            var validDuration = double.TryParse(resultString, out var duration);
            return validDuration ? duration : 0;
        }, stoppingToken);
    }

    private static void _checkSourceFile(string sourcePath)
    {
        if (!File.Exists(sourcePath))
        {
            throw new Exception($"沒辦法在以下路徑取得來源檔案：{sourcePath}");
        }
    }
}
