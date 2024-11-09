#region

#endregion

namespace CppPad.ScriptFile.Interface;

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
        TemplatesChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public void Delete(string templateName)
    {
        TemplatesChanged?.Invoke(this, EventArgs.Empty);
    }

    public Task<IReadOnlyList<string>> GetAllTemplatesAsync()
    {
        return Task.FromResult<IReadOnlyList<string>>([]);
    }
}