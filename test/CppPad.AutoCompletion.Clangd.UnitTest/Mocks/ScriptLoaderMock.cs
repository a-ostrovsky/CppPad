using CppPad.Common;
using CppPad.ScriptFile.Interface;

namespace CppPad.AutoCompletion.Clangd.UnitTest.Mocks;

public class ScriptLoaderMock : IScriptLoader
{
    public Task<ScriptDocument> LoadAsync(string path)
    {
        throw new NotSupportedException();
    }

    public Task SaveAsync(ScriptDocument scriptDocument)
    {
        throw new NotSupportedException();
    }

    public Task CreateCppFileAsync(ScriptDocument scriptDocument)
    {
        return Task.CompletedTask;
        ;
    }

    public string GetCppFilePath(ScriptDocument scriptDocument)
    {
        return "test.cpp";
    }
}