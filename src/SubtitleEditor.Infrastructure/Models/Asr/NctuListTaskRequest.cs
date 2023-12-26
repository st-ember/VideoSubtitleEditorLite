using SubtitleEditor.Core.Contexts;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubtitleEditor.Infrastructure.Models.Asr;

public class NctuListTaskRequest : NctuRequestBase
{
    [Column("pageIndex")]
    public int? PageIndex { get; set; }
    [Column("pageSize")]
    public int? PageSize { get; set; }
    [Column("taskStatus")]
    public int? TaskStatus { get; set; }
    [Column("title")]
    public string? Title { get; set; }
    [Column("start")]
    public string? Start { get; set; }
    [Column("end")]
    public string? End { get; set; }
    [Column("id")]
    public long? Id { get; set; }

    public NctuTaskStatusOptions? TaskStatusOption
    {
        get => TaskStatus == null ? null : (NctuTaskStatusOptions)TaskStatus;
        set => TaskStatus = value == null ? null : (int)value.Value;
    }

    public DateTime? StartDate
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Start)) { return null; }

            var array = Start.Replace("Z", "").Split('T');
            var date = DateTime.TryParse($"{array[0]} {array[1]}", out var d) ? d : default;
            return date != default ? date.AddHours(8) : null;
        }

        set
        {
            Start = value?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.000Z");
        }
    }

    public DateTime? EndDate
    {
        get
        {
            if (string.IsNullOrWhiteSpace(End)) { return null; }

            var array = End.Replace("Z", "").Split('T');
            var date = DateTime.TryParse($"{array[0]} {array[1]}", out var d) ? d : default;
            return date != default ? date.AddHours(8) : null;
        }

        set
        {
            if (value.HasValue)
            {
                var date = value.Value.Date.AddDays(1).AddSeconds(-1);
                End = date.ToString("yyyy-MM-ddTHH:mm:ss.000Z");
            }
            else
            {
                End = null;
            }
        }
    }
}