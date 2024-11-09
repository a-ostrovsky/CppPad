#region

using CppPad.Common;

#endregion

namespace CppPad.ScriptFile.Implementation;

public record ScriptDto
{
    public int Version { get; init; } = 1;

    public Identifier? Identifier { get; init; }

    public string Content { get; init; } = string.Empty;

    public IReadOnlyList<string> AdditionalIncludeDirs { get; init; } = [];

    public IReadOnlyList<string> LibrarySearchPaths { get; init; } = [];

    public IReadOnlyList<string> AdditionalEnvironmentPaths { get; init; } = [];

    public IReadOnlyList<string> StaticallyLinkedLibraries { get; init; } = [];

    public string CppStandard { get; init; } = string.Empty;

    public string OptimizationLevel { get; init; } = string.Empty;

    public string AdditionalBuildArgs { get; init; } = string.Empty;

    public string PreBuildCommand { get; init; } = string.Empty;
}