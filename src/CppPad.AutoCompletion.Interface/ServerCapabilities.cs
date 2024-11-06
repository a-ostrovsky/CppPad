namespace CppPad.AutoCompletion.Interface;

public record ServerCapabilities
{
    public IReadOnlySet<char> TriggerCharacters { get; set; } = new HashSet<char>();
}