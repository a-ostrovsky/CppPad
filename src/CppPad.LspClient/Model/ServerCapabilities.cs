namespace CppPad.LspClient.Model;

public record ServerCapabilities
{
    public IReadOnlySet<char> TriggerCharacters { get; set; } = new HashSet<char>();
}
