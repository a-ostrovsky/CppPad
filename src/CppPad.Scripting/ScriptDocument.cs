using CppPad.UniqueIdentifier;

namespace CppPad.Scripting;

public record ScriptDocument
{
    public ScriptData Script { get; init; } = new();

    public Identifier Identifier { get; init; } = Identifier.Empty;
    
    public string? FileName { get; init; }
}