using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace SubtitleEditor.Infrastructure.Models.Asr;

public abstract class NctuRequestBase
{
    public virtual Dictionary<string, string> ToQueryData()
    {
        var properties = GetType().GetProperties()
            .Where(prop => prop.CanWrite && prop.CanRead && prop.GetCustomAttribute<ColumnAttribute>() != null);
        var queryData = new Dictionary<string, string>();

        foreach (var property in properties)
        {
            var value = property.GetValue(this)?.ToString() ?? "";
            if (!string.IsNullOrEmpty(value))
            {
                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute != null && !string.IsNullOrEmpty(columnAttribute.Name))
                {
                    queryData.Add(columnAttribute.Name!, value);
                }
            }
        }

        return queryData;
    }
}
