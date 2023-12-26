namespace SubtitleEditor.Web.Infrastructure.Models.Benchmark;

public class BenchmarkModel
{
    public string ArgumentTemplate { get; set; } = string.Empty;
    public double Length { get; set; }

    public DateTime Start { get; set; } = DateTime.Now;
    public DateTime? PullRawFileFromAsr { get; set; }
    public DateTime? PullRawFileFromLocal { get; set; }
    public DateTime? SavedRawFile { get; set; }
    public DateTime? StartedConvert { get; set; }
    public DateTime? CompletedConvert { get; set; }
    public string? Output { get; set; }
    public bool? Success { get; set; }

    public double? TransferCost => SavedRawFile.HasValue ? (SavedRawFile.Value - Start).TotalSeconds : null;
    public double? ConvertCost => CompletedConvert.HasValue && StartedConvert.HasValue ? (CompletedConvert.Value - StartedConvert.Value).TotalSeconds : null;

    public async Task DoRetrieveRawFileFromAsr(Func<Task> func)
    {
        PullRawFileFromAsr = DateTime.Now;
        await func();
        SavedRawFile = DateTime.Now;
    }

    public async Task DoRetrieveRawFileFromLocal(Func<Task> func)
    {
        PullRawFileFromLocal = DateTime.Now;
        await func();
        SavedRawFile = DateTime.Now;
    }

    public async Task DoConvertFile(Func<Task> func)
    {
        StartedConvert = DateTime.Now;
        await func();
        CompletedConvert = DateTime.Now;
    }
}
