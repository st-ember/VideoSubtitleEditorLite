using SubtitleEditor.Core.Contexts;

namespace SubtitleEditor.Core.Attributes;

/// <summary>
/// 設定指定的 Action 可被 Permission 控制。
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class IsPermissionActionAttribute : Attribute
{
    public PermissionType[] PermissionTypes { get; set; } = Array.Empty<PermissionType>();
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "一般";

    public IsPermissionActionAttribute() { }

    public IsPermissionActionAttribute(string description, PermissionType permissionType)
    {
        Description = description;
        PermissionTypes = new PermissionType[] { permissionType };
    }

    public IsPermissionActionAttribute(string description, params PermissionType[] permissionTypes)
    {
        Description = description;
        PermissionTypes = permissionTypes;
    }

    public IsPermissionActionAttribute(PermissionType permissionType)
    {
        PermissionTypes = new PermissionType[] { permissionType };
    }

    public IsPermissionActionAttribute(params PermissionType[] permissionTypes)
    {
        PermissionTypes = permissionTypes;
    }
}