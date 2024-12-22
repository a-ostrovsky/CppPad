namespace CppPad.Scripting;

public record ScriptData
{
    public string Content { get; init; } = string.Empty;

    public CppBuildSettings BuildSettings { get; init; } = new();
}