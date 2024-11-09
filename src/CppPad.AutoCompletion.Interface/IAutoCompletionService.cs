#region

using CppPad.ScriptFile.Interface;

#endregion

namespace CppPad.AutoCompletion.Interface;

public interface IAutoCompletionService
{
    Task OpenFileAsync(string filePath, string content);

    Task CloseFileAsync(string filePath);

    Task<AutoCompletionItem[]> GetCompletionsAsync(string filePath, int line, int character);

    Task UpdateContentAsync(string filePath, string content);

    Task UpdateSettingsAsync(string filePath, Script script);

    Task<ServerCapabilities> RetrieveServerCapabilitiesAsync();

    // TODO: Not yet used
    // event EventHandler<DiagnosticsReceivedEventArgs>? OnDiagnosticsReceived;
}

public class DummyAutoCompletionService : IAutoCompletionService
{
    public Task OpenFileAsync(string filePath, string fileContent)
    {
        return Task.CompletedTask;
    }

    public Task<AutoCompletionItem[]> GetCompletionsAsync(string filePath, int line, int character)
    {
        return Task.FromResult(Array.Empty<AutoCompletionItem>());
    }

    public Task UpdateContentAsync(string filePath, string content)
    {
        return Task.CompletedTask;
    }

    public Task UpdateSettingsAsync(string filePath, Script script)
    {
        return Task.CompletedTask;
    }

    public Task CloseFileAsync(string filePath)
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