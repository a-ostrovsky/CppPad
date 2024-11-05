namespace CppPad.AutoCompletion.Interface;

public interface IAutoCompletionService
{
    Task OpenFileAsync(string filePath, string fileContent);

    Task CloseFileAsync(string filePath);

    Task<AutoCompletionItem[]> GetCompletionsAsync(string filePath, int line, int character);

    Task DidChangeAsync(string filePath, string newText);

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

    // TODO: Not yet used
    //public event EventHandler<DiagnosticsReceivedEventArgs>? OnDiagnosticsReceived;
}