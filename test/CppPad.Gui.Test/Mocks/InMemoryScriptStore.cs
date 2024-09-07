using CppPad.ScriptFile.Interface;
using CppPad.ScriptFileLoader.Interface;
using System.Collections.Concurrent;
using System.Text.Json;

namespace CppPad.Gui.Test.Mocks;

public class InMemoryScriptStore : IScriptLoader
{
    private readonly ConcurrentDictionary<string, string> _serializedScripts = new(StringComparer.OrdinalIgnoreCase);

    public async Task<Script> LoadAsync(string path)
    {
        if (!_serializedScripts.TryGetValue(path, out var serializedScript))
        {
            throw new FileNotFoundException($"Script '{path}' not found.");
        }

        return await Task.FromResult(Deserialize(serializedScript));
    }

    public async Task SaveAsync(string path, Script script)
    {
        var serializedScript = Serialize(script);
        _serializedScripts[path] = serializedScript;
        await Task.CompletedTask;
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