using CppPad.ScriptFile.Interface;
using CppPad.ScriptFileLoader.Interface;
using System.Collections.Concurrent;
using System.Text.Json;

namespace CppPad.Gui.UnitTest.Mocks;

public class InMemoryTemplateStore : ITemplateLoader
{
    private readonly ConcurrentDictionary<string, string> _serializedTemplates = new(StringComparer.OrdinalIgnoreCase);

    public event EventHandler<EventArgs>? TemplatesChanged;

    public async Task<Script> LoadAsync(string templateName)
    {
        if (!_serializedTemplates.TryGetValue(templateName, out var serializedScript))
        {
            throw new FileNotFoundException($"Template '{templateName}' not found.");
        }

        return await Task.FromResult(Deserialize(serializedScript));
    }

    public async Task SaveAsync(string templateName, Script script)
    {
        var serializedScript = Serialize(script);
        _serializedTemplates[templateName] = serializedScript;
        TemplatesChanged?.Invoke(this, EventArgs.Empty);
        await Task.CompletedTask;
    }

    public void Delete(string templateName)
    {
        if (_serializedTemplates.TryRemove(templateName, out _))
        {
            TemplatesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task<IReadOnlyList<string>> GetAllTemplatesAsync()
    {
        return await Task.FromResult(_serializedTemplates.Keys.ToList().AsReadOnly());
    }

    private static string Serialize(Script script)
    {
        return JsonSerializer.Serialize(script);
    }

    private static Script Deserialize(string content)
    {
        return JsonSerializer.Deserialize<Script>(content)!;
    }
}