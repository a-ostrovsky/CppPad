using CppPad.LspClient.Model;
using CppPad.Scripting;

namespace CppPad.CodeAssistance;

public interface ICodeAssistant
{
    Task OpenFileAsync(ScriptDocument scriptDocument);

    Task CloseFileAsync(ScriptDocument document);

    Task<AutoCompletionItem[]> GetCompletionsAsync(ScriptDocument document, Position position);

    Task<PositionInFile[]> GetDefinitionsAsync(ScriptDocument document, Position position);

    Task UpdateContentAsync(IContentUpdate update);

    Task UpdateSettingsAsync(ScriptDocument document);

    Task<ServerCapabilities> RetrieveServerCapabilitiesAsync();

    // TODO: Not yet used
    // event EventHandler<DiagnosticsReceivedEventArgs>? OnDiagnosticsReceived;
}

public class DummyCodeAssistant : ICodeAssistant
{
    public Task OpenFileAsync(ScriptDocument scriptDocument)
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

    public Task UpdateContentAsync(IContentUpdate update)
    {
        return Task.CompletedTask;
    }

    public Task UpdateSettingsAsync(ScriptDocument document)
    {
        return Task.CompletedTask;
    }

    public Task CloseFileAsync(ScriptDocument document)
    {
        return Task.CompletedTask;
    }

    public Task<ServerCapabilities> RetrieveServerCapabilitiesAsync()
    {
        return Task.FromResult(new ServerCapabilities());
    }

    // TODO: Not yet used
    //public event EventHandler<DiagnosticsReceivedEventArgs>? OnDiagnosticsReceived;
}
