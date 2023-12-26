using Microsoft.AspNetCore.Authorization;
using SubtitleEditor.Core.Contexts;
using System.Data;

namespace SubtitleEditor.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class CustomAuthorizeAttribute : AuthorizeAttribute
{
    public CustomAuthorizeAttribute(params PermissionType[] permissionTypes)
    {
        Roles = string.Join(',', new PermissionType[] { PermissionType.SystemAdmin }.Concat(permissionTypes).Distinct().Select(o => o.ToString()));
    }
}
