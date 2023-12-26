using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace SubtitleEditor.Infrastructure.Test;

[TestClass]
public class FFMpegTest
{
    public FFMpegTest()
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

        var serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task TestProbe()
    {
        var ffprobePath = Path.Combine(Environment.CurrentDirectory, "FFMpeg", "ffprobe.exe");
        var sourceFilePath = Path.Combine(Environment.CurrentDirectory, "TestFiles", "media.mp4");

        var ffprobeExists = File.Exists(ffprobePath);
        var sourceFileExists = File.Exists(sourceFilePath);

        var processStartInfo = new ProcessStartInfo()
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            FileName = ffprobePath,
            Arguments = $"-i \"{sourceFilePath}\" -show_entries format=duration -v quiet"
        };

        var value = await Task.Run(async () =>
        {
            var process = Process.Start(processStartInfo)!;
            process.WaitForExit();

            await Task.Delay(500);
            var output = process.StandardOutput.ReadToEnd();
            var durationLine = output.Split('\n').Where(o => o.StartsWith("duration=")).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(durationLine))
            {
                return 0;
            }

            var resultString = durationLine.Split('=').Last();
            var validDuration = double.TryParse(resultString, out var duration);
            return validDuration ? duration : 0;
        });

        Assert.IsTrue(ffprobeExists && sourceFileExists && value > 0);
    }
}
