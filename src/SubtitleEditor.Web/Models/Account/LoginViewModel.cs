namespace SubtitleEditor.Web.Models.Account;

public class LoginViewModel
{
    public LoginViewModel() { 
    
    }

    public LoginViewModel(bool asrAccess, bool? hasKey)
    {
        this.AsrAccess = asrAccess;
        this.HasKey = hasKey;
    }

    public string? Account { get; set; }
    public string? Password { get; set; }
    public string? CaptchaCode { get; set; }
    public string? Error { get; set; }
    public bool AsrAccess { get; set; } = false;
    public bool? HasKey { get; set; }
}
