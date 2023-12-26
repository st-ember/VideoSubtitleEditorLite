using Microsoft.AspNetCore.Authentication.Cookies;

namespace SubtitleEditor.Web.AuthenticizeObjects;

public class CookieManager : ICookieManager
{
    private readonly ICookieManager _concreteManager;

    public CookieManager()
    {
        _concreteManager = new ChunkingCookieManager();
    }

    public void AppendResponseCookie(HttpContext context, string key, string? value, CookieOptions options)
    {
        options.Domain = _removeSubdomain(context.Request.Host.Host); // Set the Cookie Domain using the request from host
        _concreteManager.AppendResponseCookie(context, key, value, options);
    }

    public void DeleteCookie(HttpContext context, string key, CookieOptions options)
    {
        options.Domain = _removeSubdomain(context.Request.Host.Host);
        _concreteManager.DeleteCookie(context, key, options);
    }

    public string? GetRequestCookie(HttpContext context, string key)
    {
        return _concreteManager.GetRequestCookie(context, key);
    }

    private static string _removeSubdomain(string host)
    {
        var splitHostname = host.Split('.');
        if (splitHostname.Length == 4 && splitHostname.All(o => int.TryParse(o, out int _)))
        {
            return host;
        }

        return splitHostname.Length > 1 ? string.Join(".", splitHostname.Skip(1)) : host;
    }
}
