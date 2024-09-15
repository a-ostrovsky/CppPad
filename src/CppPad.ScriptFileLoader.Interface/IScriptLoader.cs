using CppPad.ScriptFile.Interface;

namespace CppPad.ScriptFileLoader.Interface;

public interface IScriptLoader
{
    Task<Script> LoadAsync(string path);

    Task SaveAsync(string path, Script script);
}

public class DummyScriptLoader : IScriptLoader
{
    public Task<Script> LoadAsync(string path)
    {
        return Task.FromResult(new Script());
    }

    public Task SaveAsync(string path, Script script)
    {
        return Task.CompletedTask;
    }
}