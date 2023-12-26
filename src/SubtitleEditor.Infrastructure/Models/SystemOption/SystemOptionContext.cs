namespace SubtitleEditor.Infrastructure.Models.SystemOption;

public class SystemOptionContext : Dictionary<string, SystemOptionModel>
{
    public SystemOptionContext() { }

    public SystemOptionContext(IEnumerable<SystemOptionModel> systemOptionModels)
    {
        foreach (var model in systemOptionModels)
        {
            Add(model.Name, model);
        }
    }

    public SystemOptionModel? FirstOrDefault()
    {
        return this.Any() ? this.First().Value : null;
    }

    public SystemOptionModel? Get(string name)
    {
        return ContainsKey(name) ? this[name] : null;
    }
}
