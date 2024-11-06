namespace CppPad.AutoCompletion.Interface;

public interface IAutoCompletionService
{
    Task OpenFileAsync(string filePath, string fileContent);

    Task CloseFileAsync(string filePath);

    Task<AutoCompletionItem[]> GetCompletionsAsync(string filePath, int line, int character);

    Task DidChangeAsync(string filePath, string newText);
    
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

    public Task DidChangeAsync(string filePath, string newText)
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