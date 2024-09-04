#region

using CppPad.ScriptFile.Interface;

#endregion

namespace CppPad.ScriptFileLoader.Interface;

public interface ITemplateLoader
{
    event EventHandler<EventArgs> TemplatesChanged;

    Task<Script> LoadAsync(string templateName);

    Task SaveAsync(string templateName, Script script);

    void Delete(string templateName);

    Task<IReadOnlyList<string>> GetAllTemplatesAsync();
}

public class DummyTemplateLoader : ITemplateLoader
{
    public event EventHandler<EventArgs>? TemplatesChanged;

    public Task<Script> LoadAsync(string templateName)
    {
        return Task.FromResult(new Script());
    }

    public Task SaveAsync(string templateName, Script script)
    {
        return Task.CompletedTask;
    }

    public void Delete(string templateName)
    {
    }

    public Task<IReadOnlyList<string>> GetAllTemplatesAsync()
    {
        return Task.FromResult<IReadOnlyList<string>>([]);
    }
}