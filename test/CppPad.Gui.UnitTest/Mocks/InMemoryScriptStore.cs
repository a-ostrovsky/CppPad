#region

using System.Collections.Concurrent;
using System.Text.Json;
using CppPad.ScriptFile.Interface;

#endregion

namespace CppPad.Gui.UnitTest.Mocks;

public class InMemoryScriptStore : IScriptLoader
{
    private readonly ConcurrentDictionary<string, string> _serializedScripts =
        new(StringComparer.OrdinalIgnoreCase);

    public async Task<ScriptDocument> LoadAsync(string path)
    {
        if (!_serializedScripts.TryGetValue(path, out var serializedScriptDocument))
        {
            throw new FileNotFoundException($"Script '{path}' not found.");
        }

        var result = Deserialize(serializedScriptDocument);
        return await Task.FromResult(result);
    }

    public Task CreateCppFileAsync(ScriptDocument scriptDocument)
    {
        return Task.CompletedTask;
    }
    
    public string GetCppFilePath(ScriptDocument scriptDocument)
    {
        return "file.cpp";
    }

    public async Task SaveAsync(ScriptDocument scriptDocument)
    {
        var serializedScript = Serialize(scriptDocument);
        _serializedScripts[scriptDocument.FileName!] = serializedScript;
        await Task.CompletedTask;
    }

    public IReadOnlyList<string> GetFileNames()
    {
        return _serializedScripts.Keys.ToList();
    }

    private static string Serialize(ScriptDocument script)
    {
        return JsonSerializer.Serialize(script);
    }

    private static ScriptDocument Deserialize(string content)
    {
        return JsonSerializer.Deserialize<ScriptDocument>(content)!;
    }
}