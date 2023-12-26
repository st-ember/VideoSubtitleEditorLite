using Microsoft.AspNetCore.Mvc;

namespace SubtitleEditor.Web.Controllers;

public class EmptyController : AuthorizedController
{
    public IActionResult Index()
    {
        return View();
    }
}
