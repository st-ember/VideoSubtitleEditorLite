using SubtitleEditor.Core.Attributes;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using System.Reflection;

namespace SubtitleEditor.Infrastructure.Models.Permission;

public class PermissionContext
{
    public Guid UserId { get; }
    public PermissionType[] GroupTypes { get; } = Array.Empty<PermissionType>();
    public SystemAction[] Actions { get; private set; } = Array.Empty<SystemAction>();

    public bool IsSystemAdmin => GroupTypes.Any(o => o == PermissionType.SystemAdmin);

    public PermissionContext(Guid userId)
    {
        UserId = userId;
    }

    public PermissionContext(Guid userId, IEnumerable<PermissionType?> groupTypes, IEnumerable<SystemAction> actions)
    {
        UserId = userId;
        GroupTypes = groupTypes
            .Where(o => o.HasValue)
            .Select(o => o!.Value)
            .ToArray();

        if (!GroupTypes.Any())
        {
            Actions = actions.Distinct().ToArray();
            return;
        }

        var enumType = typeof(SystemAction);
        var permissions = EnumHelper.ListAllEnum<SystemAction>()
            .Select(o => new { Item = o, Attribute = enumType.GetRuntimeField(o.ToString())?.GetCustomAttribute<IsPermissionActionAttribute>() })
            .Where(o => o.Attribute != null && o.Attribute.PermissionTypes.Any());

        var additionalSystemActions = new List<SystemAction>();

        foreach (var permission in permissions)
        {
            if (permission.Attribute!.PermissionTypes.Any(o => GroupTypes.Contains(o)))
            {
                additionalSystemActions.Add(permission.Item);
            }
        }

        Actions = actions.Concat(additionalSystemActions).Distinct().ToArray();
    }

    public bool Contains(SystemAction action)
    {
        return IsSystemAdmin || Actions.Contains(action);
    }

    public bool ContainsAll(params SystemAction[] actions)
    {
        return IsSystemAdmin || actions.All(o => Actions.Contains(o));
    }

    public bool ContainsAny(params SystemAction[] actions)
    {
        return IsSystemAdmin || actions.Any(o => Actions.Contains(o));
    }
}
