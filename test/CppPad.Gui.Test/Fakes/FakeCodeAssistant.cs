using CppPad.CodeAssistance;
using CppPad.LspClient.Model;
using CppPad.Scripting;

namespace CppPad.Gui.Tests.Fakes;

public class FakeCodeAssistant : ICodeAssistant
{
    public Task OpenFileAsync(ScriptDocument scriptDocument)
    {
        return Task.CompletedTask;
    }

    public Task CloseFileAsync(ScriptDocument document)
    {
        return Task.CompletedTask;
    }

    public Task<AutoCompletionItem[]> GetCompletionsAsync(
        ScriptDocument document,
        Position position
    )
    {
        return Task.FromResult(Array.Empty<AutoCompletionItem>());
    }

    public Task<PositionInFile[]> GetDefinitionsAsync(ScriptDocument document, Position position)
    {
        return Task.FromResult(Array.Empty<PositionInFile>());
    }

    public Task UpdateContentAsync(ScriptDocument document)
    {
        return Task.CompletedTask;
    }

    public Task UpdateSettingsAsync(ScriptDocument document)
    {
        return Task.CompletedTask;
    }

    public Task<ServerCapabilities> RetrieveServerCapabilitiesAsync()
    {
        return Task.FromResult(new ServerCapabilities());
    }
}
