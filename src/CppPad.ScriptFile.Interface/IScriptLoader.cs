#region

using CppPad.Common;

#endregion

namespace CppPad.ScriptFile.Interface;

public interface IScriptLoader
{
    Task<ScriptDocument> LoadAsync(string path);

    Task SaveAsync(ScriptDocument scriptDocument);
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
}