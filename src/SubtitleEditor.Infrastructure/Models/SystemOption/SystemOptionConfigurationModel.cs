namespace SubtitleEditor.Infrastructure.Models.SystemOption;

public class SystemOptionConfigurationModel
{
    public Database.Entities.SystemOption[] DefaultOptions { get; set; } = Array.Empty<Database.Entities.SystemOption>();
    public Database.Entities.SystemOption[] Options { get; set; } = Array.Empty<Database.Entities.SystemOption>();
}