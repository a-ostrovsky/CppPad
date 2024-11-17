#region

using CppPad.Common;

#endregion

namespace CppPad.ScriptFile.Interface;

public interface IScriptLoader
{
    Task<ScriptDocument> LoadAsync(string path);

    Task SaveAsync(ScriptDocument scriptDocument);

    Task CreateCppFileAsync(ScriptDocument scriptDocument);

    string GetCppFilePath(ScriptDocument scriptDocument);
}

public class DummyScriptLoader : IScriptLoader
{
    public Task<ScriptDocument> LoadAsync(string path)
    {
        return Task.FromResult(new ScriptDocument
        {
            Identifier = Identifier.Empty,
            Script = new Script()
        });
    }

    public Task SaveAsync(ScriptDocument scriptDocument)
    {
        return Task.CompletedTask;
    }

    public Task CreateCppFileAsync(ScriptDocument scriptDocument)
    {
        return Task.CompletedTask;
    }

    public string GetCppFilePath(ScriptDocument scriptDocument)
    {
        return "file.cpp";
    }
}