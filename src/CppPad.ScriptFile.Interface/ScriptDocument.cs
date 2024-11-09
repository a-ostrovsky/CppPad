using CppPad.Common;

namespace CppPad.ScriptFile.Interface;

public record ScriptDocument
{
    public required Identifier Identifier { get; init; }

    public required Script Script { get; init; }

    public string? FileName { get; set; }
}