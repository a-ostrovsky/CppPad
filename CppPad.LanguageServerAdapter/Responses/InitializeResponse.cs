namespace CppPad.LanguageServerAdapter.Responses;

public class InitializeResponse
{
    public string Jsonrpc { get; init; } = string.Empty;
    public int Id { get; init; }
    public InitializeResult Result { get; init; } = new();
}

public class InitializeResult
{
    public ServerCapabilities Capabilities { get; init; } = new();
}

public class ServerCapabilities
{
    public CompletionProvider CompletionProvider { get; init; } = new();
}

public class CompletionProvider
{
    public string[] TriggerCharacters { get; init; } = [];
}