using SubtitleEditor.Core.Attributes;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Models;
using System.ComponentModel;
using System.Reflection;

namespace SubtitleEditor.Core.Helpers;

/// <summary>
/// 列舉元素的 Helper
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// 取得列舉元素的 Name 內容。
    /// </summary>
    /// <param name="value">列舉元素</param>
    public static string GetName(this Enum value)
    {
        var field = value.GetType().GetRuntimeField(value.ToString());
        if (field == null)
        {
            return value.ToString();
        }

        var attr = field.GetCustomAttributes().FirstOrDefault();

        return field.GetCustomAttributes()
            .Select(attr => attr != null && attr is NameAttribute descAttribute ? descAttribute.Name : null)
            .Where(desc => desc != null)
            .FirstOrDefault() ?? value.ToString();
    }

    /// <summary>
    /// 取得列舉元素的 Description 內容。
    /// </summary>
    /// <param name="value">列舉元素</param>
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetRuntimeField(value.ToString());
        if (field == null)
        {
            return value.ToString();
        }

        var attr = field.GetCustomAttributes().FirstOrDefault();

        if (field.GetCustomAttributes().Any(attr => attr is NameAttribute nameAttr && nameAttr.Description != null))
        {
            return field.GetCustomAttributes()
                .Select(attr => attr != null && attr is NameAttribute nameAttr ? nameAttr.Description : null)
                .Where(desc => desc != null)
                .FirstOrDefault() ?? value.ToString();
        }

        return field.GetCustomAttributes()
            .Select(attr => attr != null && attr is DescriptionAttribute descAttribute ? descAttribute.Description : null)
            .Where(desc => desc != null)
            .FirstOrDefault() ?? value.ToString();
    }

    /// <summary>
    /// 取得列舉元素的 ActionMessage 內容。
    /// </summary>
    /// <param name="value">列舉元素</param>
    /// <param name="target"></param>
    /// <param name="field"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <param name="message"></param>
    public static string GetActionMessage(this Enum value, string? target = "", string? field = "", string? before = "", string? after = "", string? message = "")
    {
        var attrs = value.GetType().GetRuntimeField(value.ToString())?.GetCustomAttributes().ToArray() ?? Array.Empty<Attribute>();
        var attr = attrs
            .Where(attr => attr != null && attr is ActionMessageAttribute)
            .Select(attr => (ActionMessageAttribute)attr)
            .FirstOrDefault();
        var actionMessage = attr != null ? attr.Message : value.ToString();

        if (string.IsNullOrEmpty(target))
        {
            actionMessage = actionMessage.Split(new string[] { "「$target」", " and " }, StringSplitOptions.RemoveEmptyEntries).First();
        }
        else if (string.IsNullOrEmpty(field))
        {
            actionMessage = actionMessage
                .Split(new string[] { " $field ", " and " }, StringSplitOptions.RemoveEmptyEntries)
                .First()
                .Replace("$target", target);
        }
        else if (string.IsNullOrEmpty(before) && string.IsNullOrEmpty(after))
        {
            actionMessage = actionMessage.Split("從").First().Replace("$target", target).Replace("$field", field);
        }
        else
        {
            var adeptedBefore = string.IsNullOrEmpty(before) ? "(無)" : before;
            var adeptedAfter = string.IsNullOrEmpty(after) ? "(無)" : after;
            actionMessage = actionMessage
                .Replace("$target", target)
                .Replace("$field", $"<b>{field}</b>")
                .Replace("$before", adeptedBefore)
                .Replace("$after", $"<b>{adeptedAfter}</b>");
        }

        if (!string.IsNullOrEmpty(message))
        {
            actionMessage = $"{actionMessage}且包含訊息：{message}";
        }

        if (!actionMessage.EndsWith(".") && !actionMessage.EndsWith("。") && !actionMessage.EndsWith("!") && !actionMessage.EndsWith("！"))
        {
            actionMessage = $"{actionMessage}。";
        }

        return actionMessage;
    }

    /// <summary>
    /// 取得列舉元素的別名清單，這些別名可以以 Alias 標籤進行指定。
    /// </summary>
    /// <param name="value">列舉元素</param>
    public static IEnumerable<string> GetAliases(this Enum value)
    {
        var field = value.GetType().GetRuntimeField(value.ToString());
        if (field == null)
        {
            return Array.Empty<string>();
        }

        return field.GetCustomAttributes()
            .Select(attr => attr != null && attr is AliasAttribute aliasAttribute ? aliasAttribute.Aliases : null)
            .Where(aliases => aliases != null)
            .SelectMany(aliases => aliases!);
    }

    private static int _getNumber(this Enum value)
    {
        return Convert.ToInt32(value);
    }

    private static int _getIndex(this Enum value)
    {
        var attrs = value.GetType().GetRuntimeField(value.ToString())?.GetCustomAttributes();
        var indexs = attrs?
            .Select(attr => attr is IndexAttribute descAttr ? descAttr.Index : -1)
            .Where(o => o >= 0)
            .ToArray();

        return indexs != null && indexs.Length > 0 ? indexs.First() : Convert.ToInt32(value);
    }

    /// <summary>
    /// 列出列舉中的所有項目
    /// </summary>
    public static IEnumerable<T> ListAllEnum<T>(this T _) where T : Enum
    {
        return ListAllEnum<T>();
    }

    /// <summary>
    /// 列出列舉中的所有項目
    /// </summary>
    public static IEnumerable<T> ListAllEnum<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    /// <summary>
    /// 將列舉中的所有元素依照設定的 Index 排序後，轉換成下拉式選單項目。
    /// </summary>
    public static IEnumerable<SelectOption> ConvertToSelectOption<T>(this T _) where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>()
            .OrderBy(o => o._getIndex())
            .Select(o => new SelectOption(o.GetName(), o._getNumber().ToString()));
    }

    /// <summary>
    /// 將列舉中的所有元素依照設定的 Index 排序後，轉換成下拉式選單項目。
    /// </summary>
    public static IEnumerable<SelectOption> ConvertToSelectOption<T>(this T _, Func<T, string> convetFunc) where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>()
            .OrderBy(o => o._getIndex())
            .Select(o => new SelectOption(o.GetName(), convetFunc(o)));
    }

    /// <summary>
    /// 將文字轉換成列舉型別。
    /// </summary>
    /// <typeparam name="T">列舉型別</typeparam>
    /// <param name="enumObject">列舉型別的預設值</param>
    /// <param name="input">要轉換的文字</param>
    public static T Parse<T>(this T enumObject, string? input) where T : Enum
    {
        TryParse(enumObject, input, out T output);
        return output;
    }

    /// <summary>
    /// 嘗試將文字轉換成列舉型別，失敗的話轉換結果將回傳預設值。
    /// </summary>
    /// <typeparam name="T">列舉型別</typeparam>
    /// <param name="enumObject">列舉型別的預設值</param>
    /// <param name="input">要轉換的文字</param>
    /// <param name="output">轉換結果</param>
    /// <returns>是否有成功完成轉換</returns>
    public static bool TryParse<T>(this T enumObject, string? input, out T output) where T : Enum
    {
        if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int number))
        {
            try
            {
                output = (T)(object)number;
                return true;
            }
            catch
            {
                output = enumObject;
                return false;
            }
        }
        else
        {
            var matchs = Enum.GetValues(typeof(T))
                .Cast<T>()
                .Where(o => o.ToString() == input || o.GetName() == input || o.GetName() == input || o.GetAliases().Contains(input));

            if (matchs.Any())
            {
                output = matchs.First();
                return true;
            }
            else
            {
                output = enumObject;
                return false;
            }
        }
    }

    /// <summary>
    /// 檢查指定的系統動作是否與權限綁在一起。
    /// </summary>
    /// <param name="value"></param>
    public static bool IsPermissionAction(this SystemAction value)
    {
        return value.GetType().GetRuntimeField(value.ToString())?.GetCustomAttribute<IsPermissionActionAttribute>() != null;
    }
}
