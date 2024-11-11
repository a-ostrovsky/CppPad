#region

using CppPad.ScriptFile.Interface;

#endregion

namespace CppPad.AutoCompletion.Interface;

public interface IAutoCompletionService
{
    Task OpenFileAsync(ScriptDocument scriptDocument);

    Task CloseFileAsync(ScriptDocument document);

    Task<AutoCompletionItem[]> GetCompletionsAsync(ScriptDocument document, int line, int character);

    Task UpdateContentAsync(ScriptDocument document);

    Task UpdateSettingsAsync(ScriptDocument document);

    Task<ServerCapabilities> RetrieveServerCapabilitiesAsync();

    // TODO: Not yet used
    // event EventHandler<DiagnosticsReceivedEventArgs>? OnDiagnosticsReceived;
}

public class DummyAutoCompletionService : IAutoCompletionService
{
    public Task OpenFileAsync(ScriptDocument scriptDocument)
    {
        return Task.CompletedTask;
    }

    public Task<AutoCompletionItem[]> GetCompletionsAsync(ScriptDocument document, int line, int character)
    {
        return Task.FromResult(Array.Empty<AutoCompletionItem>());
    }

    public Task UpdateContentAsync(ScriptDocument document)
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