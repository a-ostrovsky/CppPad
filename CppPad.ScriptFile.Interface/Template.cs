using CppPad.CompilerAdapter.Interface;

namespace CppPad.ScriptFile.Interface;

public record Template
{
    public string Content { get; init; } = string.Empty;

    public BuildArgs BuildArgs { get; init; } = new();
}