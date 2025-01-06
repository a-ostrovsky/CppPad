using CppPad.UniqueIdentifier;

namespace CppPad.Scripting;

public record ScriptDocument
{
    public ScriptData Script { get; init; } = new();

    public Identifier Identifier { get; init; } = IdentifierGenerator.GenerateUniqueId();

    public string? FileName { get; set; }
}
