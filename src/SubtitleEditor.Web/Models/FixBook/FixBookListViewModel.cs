namespace SubtitleEditor.Web.Models.FixBook;

public class FixBookListViewModel
{
    public string ModelName { get; set; } = string.Empty;

    public FixBookPageModel[] Pages { get; set; } = Array.Empty<FixBookPageModel>();

    public bool AsrAccess { get; set; } = false;
}
