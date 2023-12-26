namespace SubtitleEditor.Web.Infrastructure.Models.Log;

public interface ILogListCondition
{
    public string? Start { get; set; }
    public string? End { get; set; }
    public string? Actions { get; set; }
    public string? Target { get; set; }
    public string? IPAddress { get; set; }
    public string? User { get; set; }
    public bool? IsActionSuccess { get; set; }
}
